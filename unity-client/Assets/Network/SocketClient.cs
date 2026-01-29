using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ShipBattle.Network
{
    /// <summary>
    /// WebSocket client for real-time communication with backend.
    /// Handles connection, message sending/receiving, and event dispatching.
    /// Modified for Phase 2 - Added game:input and state:snapshot handlers.
    /// </summary>
    public class SocketClient
    {
        private ClientWebSocket webSocket;
        private CancellationTokenSource cancellationTokenSource;
        private string serverUrl = "ws://localhost:3000/pvp";
        private string authToken;
        
        // Connection state
        private bool isConnected = false;
        public bool IsConnected => isConnected;

        // Events for matchmaking (Phase 1)
        public event Action<QueueStatusData> OnQueueStatus;
        public event Action<MatchFoundData> OnMatchFound;
        public event Action<MatchReadyData> OnMatchReady;
        public event Action<MatchEndData> OnMatchEnd;
        
        // Events for gameplay (Phase 2)
        public event Action<GameSnapshot> OnStateSnapshot;
        public event Action OnConnected;
        public event Action OnDisconnected;

        public async Task ConnectAsync(string token)
        {
            authToken = token;
            
            try
            {
                webSocket = new ClientWebSocket();
                cancellationTokenSource = new CancellationTokenSource();
                
                string urlWithAuth = $"{serverUrl}?token={authToken}";
                Uri uri = new Uri(urlWithAuth);
                
                Debug.Log($"[SocketClient] Connecting to {serverUrl}...");
                await webSocket.ConnectAsync(uri, cancellationTokenSource.Token);
                
                isConnected = true;
                OnConnected?.Invoke();
                Debug.Log("[SocketClient] Connected successfully");
                
                // Start receiving messages
                _ = ReceiveLoop();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SocketClient] Connection failed: {e.Message}");
                isConnected = false;
                throw;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (webSocket != null && webSocket.State == WebSocketState.Open)
                {
                    cancellationTokenSource?.Cancel();
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None);
                    Debug.Log("[SocketClient] Disconnected");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SocketClient] Disconnect error: {e.Message}");
            }
            finally
            {
                isConnected = false;
                OnDisconnected?.Invoke();
                webSocket?.Dispose();
                cancellationTokenSource?.Dispose();
            }
        }

        private async Task ReceiveLoop()
        {
            byte[] buffer = new byte[8192];
            
            try
            {
                while (webSocket.State == WebSocketState.Open && !cancellationTokenSource.Token.IsCancellationRequested)
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        cancellationTokenSource.Token
                    );

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await DisconnectAsync();
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    HandleMessage(message);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SocketClient] Receive loop error: {e.Message}");
                isConnected = false;
                OnDisconnected?.Invoke();
            }
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
                
                if (!doc.TryGetValue("data", out JToken dataToken))
                {
                    Debug.LogWarning($"[SocketClient] Message missing 'data' field for type: {eventType}");
                    return;
                }

                string dataJson = dataToken.ToString();
                
                // Dispatch based on event type
                switch (eventType)
                {
                    case "queue:status":
                        var queueStatus = JsonConvert.DeserializeObject<QueueStatusData>(dataJson);
                        OnQueueStatus?.Invoke(queueStatus);
                        break;
                        
                    case "match:found":
                        var matchFound = JsonConvert.DeserializeObject<MatchFoundData>(dataJson);
                        OnMatchFound?.Invoke(matchFound);
                        break;
                        
                    case "match:ready":
                        var matchReady = JsonConvert.DeserializeObject<MatchReadyData>(dataJson);
                        OnMatchReady?.Invoke(matchReady);
                        break;
                        
                    case "match:end":
                        var matchEnd = JsonConvert.DeserializeObject<MatchEndData>(dataJson);
                        OnMatchEnd?.Invoke(matchEnd);
                        break;
                        
                    case "state:snapshot":
                        var snapshot = JsonConvert.DeserializeObject<GameSnapshot>(dataJson);
                        OnStateSnapshot?.Invoke(snapshot);
                        break;
                        
                    default:
                        Debug.LogWarning($"[SocketClient] Unknown event type: {eventType}");
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SocketClient] Message handling error: {e.Message}\nMessage: {message}");
            }
        }

        public async Task SendMessageAsync(string eventType, object data)
        {
            if (!isConnected || webSocket.State != WebSocketState.Open)
            {
                Debug.LogWarning($"[SocketClient] Cannot send message - not connected");
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

        // Matchmaking API
        public Task JoinQueueAsync()
        {
            return SendMessageAsync("queue:join", new { });
        }

        public Task LeaveQueueAsync()
        {
            return SendMessageAsync("queue:leave", new { });
        }

        public Task AcceptMatchAsync()
        {
            return SendMessageAsync("match:accept", new { });
        }

        // Gameplay API (Phase 2)
        public Task SendInputAsync(InputData input)
        {
            return SendMessageAsync("game:input", input);
        }
    }

    // Data structures for Phase 1 (Matchmaking)
    [Serializable]
    public class QueueStatusData
    {
        public string status { get; set; }
        public int queueSize { get; set; }
        public long timestamp { get; set; }
    }

    [Serializable]
    public class MatchFoundData
    {
        public int matchId { get; set; }
        public OpponentData opponent { get; set; }
        public long acceptDeadline { get; set; }
    }

    [Serializable]
    public class MatchReadyData
    {
        public int matchId { get; set; }
        public OpponentData opponent { get; set; }
    }

    [Serializable]
    public class MatchEndData
    {
        public int matchId { get; set; }
        public int winnerId { get; set; }
        public string reason { get; set; }
        public EloChange eloChange { get; set; }
    }

    [Serializable]
    public class OpponentData
    {
        public int id { get; set; }
        public string username { get; set; }
        public int rating { get; set; }
    }

    [Serializable]
    public class EloChange
    {
        public int oldRating { get; set; }
        public int newRating { get; set; }
        public int change { get; set; }
    }

    // Data structures for Phase 2 (Gameplay)
    [Serializable]
    public class InputData
    {
        public Vector2Data direction { get; set; }
        public bool isFiring { get; set; }
        public bool ability { get; set; }
        public long timestamp { get; set; }
    }

    [Serializable]
    public class Vector2Data
    {
        public float x { get; set; }
        public float y { get; set; }
    }

    [Serializable]
    public class GameSnapshot
    {
        public int tick { get; set; }
        public long timestamp { get; set; }
        public ShipData[] ships { get; set; }
        public BulletData[] bullets { get; set; }
    }

    [Serializable]
    public class ShipData
    {
        public string id { get; set; }
        public Vector2Data position { get; set; }
        public float rotation { get; set; }
        public float health { get; set; }
        public float shield { get; set; }
    }

    [Serializable]
    public class BulletData
    {
        public string id { get; set; }
        public Vector2Data position { get; set; }
        public Vector2Data direction { get; set; }
        public string ownerId { get; set; }
    }
}
