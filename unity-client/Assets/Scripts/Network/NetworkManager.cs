using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using PvpGame.Utils;
using PvpGame.Config;
using PvpGame.Auth;

namespace PvpGame.Network
{
    [Serializable]
    public class WebSocketMessage
    {
        public string eventName;
        public string data;
    }

    [Serializable]
    public class QueueStatusData
    {
        public int position;
        public int estimatedWait;
    }

    [Serializable]
    public class OpponentData
    {
        public int id;
        public string username;
        public int rating;
    }

    [Serializable]
    public class MatchFoundData
    {
        public int matchId;
        public OpponentData opponent;
    }

    [Serializable]
    public class MatchStartData
    {
        public int matchId;
        public OpponentData opponent;
        public string color;
    }

    [Serializable]
    public class MatchReadyData
    {
        public int matchId;
    }

    [Serializable]
    public class GameEndData
    {
        public int matchId;
        public int winner;
        public int loser;
        public int eloChange;
    }

    public class NetworkManager : Singleton<NetworkManager>
    {
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<QueueStatusData> OnQueueStatus;
        public event Action<MatchFoundData> OnMatchFound;
        public event Action<MatchStartData> OnMatchStart;
        public event Action<PvpGame.Game.GameStateData> OnGameSnapshot;
        public event Action<GameEndData> OnGameEnd;

        private ClientWebSocket webSocket;
        private CancellationTokenSource cancellationTokenSource;
        private Queue<Action> mainThreadActions = new Queue<Action>();
        private bool isConnected = false;
        private GameConfig config;

        protected override void Awake()
        {
            base.Awake();
            config = GameConfig.Instance;
        }

        private void Update()
        {
            lock (mainThreadActions)
            {
                while (mainThreadActions.Count > 0)
                {
                    mainThreadActions.Dequeue()?.Invoke();
                }
            }
        }

        public async Task<bool> ConnectAsync()
        {
            if (isConnected)
            {
                AppLogger.LogWarning("Already connected to WebSocket");
                return true;
            }

            try
            {
                AppLogger.LogNetwork($"Connecting to WebSocket: {config.websocketUrl}");

                webSocket = new ClientWebSocket();
                cancellationTokenSource = new CancellationTokenSource();

                string token = AuthManager.Instance.GetAccessToken();
                webSocket.Options.SetRequestHeader("Authorization", $"Bearer {token}");

                Uri serverUri = new Uri(config.websocketUrl);
                await webSocket.ConnectAsync(serverUri, cancellationTokenSource.Token);

                isConnected = true;
                AppLogger.LogSuccess("WebSocket connected");

                EnqueueMainThreadAction(() => OnConnected?.Invoke());

                _ = ReceiveLoopAsync();

                return true;
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"WebSocket connection failed: {ex.Message}");
                return false;
            }
        }

        private async Task ReceiveLoopAsync()
        {
            var buffer = new byte[1024 * 4];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        ProcessMessage(message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        AppLogger.LogNetwork("WebSocket closed by server");
                        await DisconnectAsync();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"WebSocket receive error: {ex.Message}");
                await DisconnectAsync();
            }
        }

        private void ProcessMessage(string message)
        {
            try
            {
                var wsMessage = JsonHelper.Deserialize<WebSocketMessage>(message);
                if (wsMessage == null)
                {
                    AppLogger.LogError($"Failed to parse WebSocket message: {message}");
                    return;
                }

                AppLogger.LogNetwork($"Received event: {wsMessage.eventName}");

                switch (wsMessage.eventName)
                {
                    case "queue:status":
                        var queueStatus = JsonHelper.Deserialize<QueueStatusData>(wsMessage.data);
                        EnqueueMainThreadAction(() => OnQueueStatus?.Invoke(queueStatus));
                        break;

                    case "match:found":
                        var matchFound = JsonHelper.Deserialize<MatchFoundData>(wsMessage.data);
                        EnqueueMainThreadAction(() => OnMatchFound?.Invoke(matchFound));
                        break;

                    case "match:start":
                        var matchStart = JsonHelper.Deserialize<MatchStartData>(wsMessage.data);
                        EnqueueMainThreadAction(() => OnMatchStart?.Invoke(matchStart));
                        break;

                    case "game:snapshot":
                        var snapshot = JsonHelper.Deserialize<PvpGame.Game.GameStateData>(wsMessage.data);
                        EnqueueMainThreadAction(() => OnGameSnapshot?.Invoke(snapshot));
                        break;

                    case "game:end":
                        var gameEnd = JsonHelper.Deserialize<GameEndData>(wsMessage.data);
                        EnqueueMainThreadAction(() => OnGameEnd?.Invoke(gameEnd));
                        break;

                    default:
                        AppLogger.LogWarning($"Unknown event: {wsMessage.eventName}");
                        break;
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"Error processing message: {ex.Message}");
            }
        }

        public async Task SendEventAsync(string eventName, object data = null)
        {
            if (!isConnected || webSocket.State != WebSocketState.Open)
            {
                AppLogger.LogError("Cannot send event: WebSocket not connected");
                return;
            }

            try
            {
                var message = new WebSocketMessage
                {
                    eventName = eventName,
                    data = data != null ? JsonHelper.Serialize(data) : "{}"
                };

                string json = JsonHelper.Serialize(message);
                byte[] bytes = Encoding.UTF8.GetBytes(json);

                await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationTokenSource.Token);

                AppLogger.LogNetwork($"Sent event: {eventName}");
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"Failed to send event: {ex.Message}");
            }
        }

        public async Task DisconnectAsync()
        {
            if (!isConnected)
            {
                return;
            }

            try
            {
                AppLogger.LogNetwork("Disconnecting WebSocket");

                isConnected = false;
                cancellationTokenSource?.Cancel();

                if (webSocket != null && webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnect", CancellationToken.None);
                }

                webSocket?.Dispose();
                webSocket = null;

                EnqueueMainThreadAction(() => OnDisconnected?.Invoke());

                AppLogger.LogNetwork("WebSocket disconnected");
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"Error during disconnect: {ex.Message}");
            }
        }

        private void EnqueueMainThreadAction(Action action)
        {
            lock (mainThreadActions)
            {
                mainThreadActions.Enqueue(action);
            }
        }

        public bool IsConnected()
        {
            return isConnected && webSocket != null && webSocket.State == WebSocketState.Open;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _ = DisconnectAsync();
        }
    }
}
