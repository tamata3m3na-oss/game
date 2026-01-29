using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ShipBattle.Network
{
    public class SocketClient
    {
        private static SocketClient instance;
        public static SocketClient Instance
        {
            get
            {
                if (instance == null) 
                {
                    instance = new SocketClient();
                    Debug.Log("[SocketClient] Instance created");
                }
                return instance;
            }
        }

        private ClientWebSocket webSocket;
        private CancellationTokenSource cancellationTokenSource;
        private string serverUrl = "ws://localhost:3000/pvp";
        private string authToken;
        
        public bool IsConnected => webSocket != null && webSocket.State == WebSocketState.Open;

        public delegate void MatchReadyHandler(MatchReadyEvent data);
        public delegate void MatchEndHandler(MatchEndEvent data);
        public delegate void StateSnapshotHandler(GameStateSnapshot snapshot);
        public delegate void ErrorHandler(string error);

        public event MatchReadyHandler OnMatchReady;
        public event MatchEndHandler OnMatchEnd;
        public event StateSnapshotHandler OnStateSnapshot;
        public event ErrorHandler OnError;
        public event Action OnConnected;
        public event Action OnDisconnected;

        private SocketClient()
        {
            Debug.Log($"[SocketClient] Initialized with server URL: {serverUrl}");
        }

        public async Task ConnectAsync(string token)
        {
            authToken = token;
            Debug.Log("[SocketClient] ConnectAsync called");
            
            try
            {
                // Cancel previous connection if any
                if (webSocket != null)
                {
                    Debug.Log("[SocketClient] Disposing previous connection");
                    cancellationTokenSource?.Cancel();
                    webSocket.Dispose();
                }

                webSocket = new ClientWebSocket();
                cancellationTokenSource = new CancellationTokenSource();
                
                string urlWithAuth = $"{serverUrl}?token={authToken}";
                Uri uri = new Uri(urlWithAuth);
                
                Debug.Log($"[SocketClient] Connecting to {serverUrl}...");
                await webSocket.ConnectAsync(uri, cancellationTokenSource.Token);
                
                Debug.Log("[SocketClient] Connected successfully");
                OnConnected?.Invoke();
                
                _ = ReceiveLoop();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SocketClient] Connection failed: {e.Message}");
                Debug.LogError($"[SocketClient] Stack trace: {e.StackTrace}");
                OnError?.Invoke(e.Message);
                throw;
            }
        }

        public async Task DisconnectAsync()
        {
            Debug.Log("[SocketClient] DisconnectAsync called");
            try
            {
                if (webSocket != null && webSocket.State == WebSocketState.Open)
                {
                    cancellationTokenSource?.Cancel();
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None);
                    Debug.Log("[SocketClient] Disconnected successfully");
                }
                else
                {
                    Debug.Log("[SocketClient] Already disconnected or not connected");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SocketClient] Disconnect error: {e.Message}");
            }
            finally
            {
                OnDisconnected?.Invoke();
                webSocket?.Dispose();
                cancellationTokenSource?.Dispose();
            }
        }

        private async Task ReceiveLoop()
        {
            byte[] buffer = new byte[8192];
            Debug.Log("[SocketClient] Receive loop started");
            
            try
            {
                while (webSocket.State == WebSocketState.Open && !cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Debug.Log("[SocketClient] Waiting for message...");
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        cancellationTokenSource.Token
                    );

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Debug.Log("[SocketClient] Received close message from server");
                        await DisconnectAsync();
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Debug.Log($"[SocketClient] Message received: {message.Substring(0, Math.Min(200, message.Length))}...");
                    HandleMessage(message);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[SocketClient] Receive loop cancelled");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SocketClient] Receive loop error: {e.Message}");
                OnDisconnected?.Invoke();
            }
            
            Debug.Log("[SocketClient] Receive loop ended");
        }

        private void HandleMessage(string message)
        {
            try
            {
                JObject doc = JObject.Parse(message);
                
                if (!doc.TryGetValue("type", out JToken typeToken)) 
                {
                    Debug.LogWarning("[SocketClient] Message missing 'type' field");
                    return;
                }
                
                string eventType = typeToken.Value<string>();
                Debug.Log($"[SocketClient] Handling event type: {eventType}");
                
                if (!doc.TryGetValue("data", out JToken dataToken)) 
                {
                    Debug.LogWarning("[SocketClient] Message missing 'data' field");
                    return;
                }
                
                string dataJson = dataToken.ToString();
                
                switch (eventType)
                {
                    case "match:ready":
                        var matchReady = JsonConvert.DeserializeObject<MatchReadyEvent>(dataJson);
                        Debug.Log($"[SocketClient] Match ready received: Opponent={matchReady.opponentUsername}");
                        OnMatchReady?.Invoke(matchReady);
                        break;
                        
                    case "match:end":
                        var matchEnd = JsonConvert.DeserializeObject<MatchEndEvent>(dataJson);
                        Debug.Log($"[SocketClient] Match end received: Winner={matchEnd.winnerId}");
                        OnMatchEnd?.Invoke(matchEnd);
                        break;
                        
                    case "state:snapshot":
                        var snapshot = JsonConvert.DeserializeObject<GameStateSnapshot>(dataJson);
                        Debug.Log($"[SocketClient] State snapshot received: Ships={snapshot?.ships?.Count ?? 0}, Bullets={snapshot?.bullets?.Count ?? 0}");
                        OnStateSnapshot?.Invoke(snapshot);
                        break;

                    case "error":
                        var error = dataJson;
                        Debug.LogError($"[SocketClient] Server error: {error}");
                        OnError?.Invoke(error);
                        break;
                        
                    default:
                        Debug.Log($"[SocketClient] Unknown event type: {eventType}");
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SocketClient] Message handling error: {e.Message}");
            }
        }

        public async void SendEvent(string eventType, object data)
        {
             await SendMessageAsync(eventType, data);
        }

        public async Task SendMessageAsync(string eventType, object data)
        {
            if (webSocket == null || webSocket.State != WebSocketState.Open)
            {
                Debug.LogWarning($"[SocketClient] Cannot send message - not connected (State: {webSocket?.State})");
                return;
            }

            try
            {
                var message = new
                {
                    type = eventType,
                    data = data
                };

                string json = JsonConvert.SerializeObject(message);
                byte[] bytes = Encoding.UTF8.GetBytes(json);

                Debug.Log($"[SocketClient] Sending event: {eventType}");
                await webSocket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    cancellationTokenSource.Token
                );
            }
            catch (Exception e)
            {
                Debug.LogError($"[SocketClient] Send message error: {e.Message}");
            }
        }
    }
}
