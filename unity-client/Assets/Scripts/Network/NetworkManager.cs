using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }
    
    public string ServerUrl = "ws://localhost:3000";
    public string PvpNamespace = "/pvp";
    
    private ClientWebSocket socket;
    private CancellationTokenSource cancellationTokenSource;
    private Queue<Action> eventQueue = new Queue<Action>();
    private bool isConnected = false;
    private string authToken = "";
    
    public UnityEvent<string> OnConnected = new UnityEvent<string>();
    public UnityEvent<string> OnDisconnected = new UnityEvent<string>();
    public UnityEvent<string> OnConnectionError = new UnityEvent<string>();
    
    public UnityEvent<QueueStatus> OnQueueStatus = new UnityEvent<QueueStatus>();
    
    public UnityEvent<MatchFoundData> OnMatchFound = new UnityEvent<MatchFoundData>();
    public UnityEvent<MatchStartData> OnMatchStart = new UnityEvent<MatchStartData>();
    public UnityEvent<GameState> OnGameSnapshot = new UnityEvent<GameState>();
    public UnityEvent<GameEndData> OnGameEnd = new UnityEvent<GameEndData>();
    
    [Serializable]
    public class QueueStatus
    {
        public int position;
        public int estimatedWait;
    }
    
    [Serializable]
    public class MatchFoundData
    {
        public int matchId;
        public OpponentData opponent;
    }
    
    [Serializable]
    public class OpponentData
    {
        public int id;
        public string username;
        public int rating;
    }
    
    [Serializable]
    public class MatchStartData
    {
        public int matchId;
        public OpponentData opponent;
        public string color;
    }
    
    [Serializable]
    public class GameState
    {
        public int matchId;
        public PlayerState player1;
        public PlayerState player2;
        public int tick;
        public long timestamp;
        public int winner;
        public string status;
    }
    
    [Serializable]
    public class PlayerState
    {
        public int id;
        public float x;
        public float y;
        public float rotation;
        public int health;
        public int shieldHealth;
        public bool shieldActive;
        public int shieldEndTick;
        public bool fireReady;
        public int fireReadyTick;
        public bool abilityReady;
        public long lastAbilityTime;
        public int damageDealt;
    }
    
    [Serializable]
    public class GameEndData
    {
        public int matchId;
        public int winner;
        public GameState finalState;
    }
    
    [Serializable]
    private class WebSocketMessage
    {
        public string type;
        public string data;
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        while (eventQueue.Count > 0)
        {
            Action action;
            lock (eventQueue)
            {
                action = eventQueue.Dequeue();
            }
            action?.Invoke();
        }
    }
    
    public async void Initialize(string token)
    {
        authToken = token;
        
        string wsUrl = ServerUrl + PvpNamespace + "?token=" + authToken;
        
        socket = new ClientWebSocket();
        cancellationTokenSource = new CancellationTokenSource();
        
        Debug.Log("Attempting WebSocket connection to: " + wsUrl);
        
        try
        {
            await socket.ConnectAsync(new Uri(wsUrl), cancellationTokenSource.Token);
            
            isConnected = true;
            QueueOnMainThread(() =>
            {
                OnConnected.Invoke("Connected to server");
                Debug.Log("WebSocket connected to server");
            });
            
            StartCoroutine(ReceiveLoop());
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to connect: " + ex.Message);
            QueueOnMainThread(() => OnConnectionError.Invoke("Connection failed: " + ex.Message));
        }
    }
    
    private IEnumerator ReceiveLoop()
    {
        var buffer = new byte[4096];
        
        while (socket.State == System.Net.WebSockets.WebSocketState.Open && !cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);
                
                if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Closing", cancellationTokenSource.Token);
                    isConnected = false;
                    QueueOnMainThread(() =>
                    {
                        OnDisconnected.Invoke("Server closed connection");
                        Debug.Log("WebSocket disconnected: Server closed connection");
                    });
                    break;
                }
                
                if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    ProcessMessage(message);
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Debug.LogError("WebSocket receive error: " + ex.Message);
                isConnected = false;
                QueueOnMainThread(() =>
                {
                    OnConnectionError.Invoke("Connection error: " + ex.Message);
                });
                break;
            }
            
            yield return null;
        }
    }
    
    private void ProcessMessage(string message)
    {
        try
        {
            WebSocketMessage wsMsg = JsonUtility.FromJson<WebSocketMessage>(message);
            
            if (wsMsg == null || string.IsNullOrEmpty(wsMsg.type))
            {
                Debug.LogWarning("Received malformed message");
                return;
            }
            
            switch (wsMsg.type)
            {
                case "queue:status":
                    QueueStatus queueStatus = JsonUtility.FromJson<QueueStatus>(wsMsg.data);
                    QueueOnMainThread(() => OnQueueStatus.Invoke(queueStatus));
                    break;
                
                case "match:found":
                    MatchFoundData matchFound = JsonUtility.FromJson<MatchFoundData>(wsMsg.data);
                    QueueOnMainThread(() => OnMatchFound.Invoke(matchFound));
                    break;
                
                case "match:start":
                    MatchStartData matchStart = JsonUtility.FromJson<MatchStartData>(wsMsg.data);
                    QueueOnMainThread(() => OnMatchStart.Invoke(matchStart));
                    break;
                
                case "game:snapshot":
                    GameState gameSnapshot = JsonUtility.FromJson<GameState>(wsMsg.data);
                    QueueOnMainThread(() => OnGameSnapshot.Invoke(gameSnapshot));
                    break;
                
                case "game:end":
                    GameEndData gameEnd = JsonUtility.FromJson<GameEndData>(wsMsg.data);
                    QueueOnMainThread(() => OnGameEnd.Invoke(gameEnd));
                    break;
                
                default:
                    Debug.LogWarning("Unknown message type: " + wsMsg.type);
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error processing message: " + ex.Message);
        }
    }
    
    private void QueueOnMainThread(Action action)
    {
        lock (eventQueue)
        {
            eventQueue.Enqueue(action);
        }
    }
    
    private async Task SendMessageAsync(string json)
    {
        if (!isConnected || socket == null || socket.State != System.Net.WebSockets.WebSocketState.Open)
        {
            Debug.LogError("Not connected to server");
            return;
        }
        
        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(new ArraySegment<byte>(buffer), System.Net.WebSockets.WebSocketMessageType.Text, true, cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to send message: " + ex.Message);
        }
    }
    
    public async void JoinQueue()
    {
        WebSocketMessage message = new WebSocketMessage
        {
            type = "queue:join",
            data = "{}"
        };
        
        string json = JsonUtility.ToJson(message);
        await SendMessageAsync(json);
        
        Debug.Log("Joined matchmaking queue");
    }
    
    public async void LeaveQueue()
    {
        WebSocketMessage message = new WebSocketMessage
        {
            type = "queue:leave",
            data = "{}"
        };
        
        string json = JsonUtility.ToJson(message);
        await SendMessageAsync(json);
        
        Debug.Log("Left matchmaking queue");
    }
    
    public async void MarkMatchReady(int matchId)
    {
        var data = new MatchReadyData { matchId = matchId };
        WebSocketMessage message = new WebSocketMessage
        {
            type = "match:ready",
            data = JsonUtility.ToJson(data)
        };
        
        string json = JsonUtility.ToJson(message);
        await SendMessageAsync(json);
        
        Debug.Log("Marked ready for match: " + matchId);
    }
    
    public async void SendGameInput(GameInputData input)
    {
        WebSocketMessage message = new WebSocketMessage
        {
            type = "game:input",
            data = JsonUtility.ToJson(input)
        };
        
        string json = JsonUtility.ToJson(message);
        await SendMessageAsync(json);
    }
    
    public async void Disconnect()
    {
        cancellationTokenSource?.Cancel();
        
        if (socket != null && socket.State == System.Net.WebSockets.WebSocketState.Open)
        {
            try
            {
                await socket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error closing WebSocket: " + ex.Message);
            }
        }
        
        socket?.Dispose();
        socket = null;
        isConnected = false;
    }
    
    public bool IsConnected()
    {
        return isConnected && socket != null && socket.State == System.Net.WebSockets.WebSocketState.Open;
    }
    
    private void OnDestroy()
    {
        Disconnect();
    }
    
    private void OnApplicationQuit()
    {
        Disconnect();
    }
    
    [Serializable]
    private class MatchReadyData
    {
        public int matchId;
    }
}

[Serializable]
public class GameInputData
{
    public int playerId;
    public long timestamp;
    public float moveX;
    public float moveY;
    public bool fire;
    public bool ability;
}
