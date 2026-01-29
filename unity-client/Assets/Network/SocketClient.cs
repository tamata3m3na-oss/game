using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using ShipBattle.Network.Models;

namespace ShipBattle.Network
{
    public class SocketClient
    {
        private static SocketClient instance;
        public static SocketClient Instance
        {
            get
            {
                if (instance == null) instance = new SocketClient();
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

        public async Task ConnectAsync(string token)
        {
            authToken = token;
            
            try
            {
                // Cancel previous connection if any
                if (webSocket != null)
                {
                    cancellationTokenSource?.Cancel();
                    webSocket.Dispose();
                }

                webSocket = new ClientWebSocket();
                cancellationTokenSource = new CancellationTokenSource();
                
                string urlWithAuth = $"{serverUrl}?token={authToken}";
                Uri uri = new Uri(urlWithAuth);
                
                Debug.Log($"[SocketClient] Connecting to {serverUrl}...");
                await webSocket.ConnectAsync(uri, cancellationTokenSource.Token);
                
                OnConnected?.Invoke();
                Debug.Log("[SocketClient] Connected successfully");
                
                _ = ReceiveLoop();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SocketClient] Connection failed: {e.Message}");
                OnError?.Invoke(e.Message);
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
                OnDisconnected?.Invoke();
            }
        }

        private void HandleMessage(string message)
        {
            try
            {
                JObject doc = JObject.Parse(message);
                
                if (!doc.TryGetValue("type", out JToken typeToken)) return;
                string eventType = typeToken.Value<string>();
                
                if (!doc.TryGetValue("data", out JToken dataToken)) return;
                string dataJson = dataToken.ToString();
                
                switch (eventType)
                {
                    case "match:ready":
                        var matchReady = JsonConvert.DeserializeObject<MatchReadyEvent>(dataJson);
                        OnMatchReady?.Invoke(matchReady);
                        Debug.Log($"[WS] match:ready received: {matchReady.opponentUsername}");
                        break;
                        
                    case "match:end":
                        var matchEnd = JsonConvert.DeserializeObject<MatchEndEvent>(dataJson);
                        OnMatchEnd?.Invoke(matchEnd);
                        Debug.Log($"[WS] match:end received: winner={matchEnd.winnerId}");
                        break;
                        
                    case "state:snapshot":
                        var snapshot = JsonConvert.DeserializeObject<GameStateSnapshot>(dataJson);
                        OnStateSnapshot?.Invoke(snapshot);
                        break;

                    case "error":
                        var error = dataJson;
                        OnError?.Invoke(error);
                        Debug.LogError($"[WS] Socket error: {error}");
                        break;
                        
                    default:
                        // Ignore unknown events or handle legacy ones if needed
                        Debug.Log($"[WS] Unknown event type: {eventType}");
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
    }
}
