using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NativeWebSocket;
using Newtonsoft.Json;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }
    
    public string ServerUrl = "ws://localhost:3000";
    public string PvpNamespace = "/pvp";
    
    private WebSocket socket;
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
        if (socket != null)
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            socket.DispatchMessageQueue();
#endif
        }
        
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
        
        socket = new WebSocket(wsUrl);
        
        socket.OnOpen = () =>
        {
            isConnected = true;
            QueueOnMainThread(() =>
            {
                OnConnected.Invoke("Connected to server");
                Debug.Log("WebSocket connected to server");
            });
        };
        
        socket.OnClose = (e) =>
        {
            isConnected = false;
            QueueOnMainThread(() =>
            {
                OnDisconnected.Invoke("Disconnected: " + e.ToString());
                Debug.Log("WebSocket disconnected: " + e);
            });
        };
        
        socket.OnError = (e) =>
        {
            QueueOnMainThread(() =>
            {
                OnConnectionError.Invoke("Error: " + e);
                Debug.LogError("WebSocket error: " + e);
            });
        };
        
        socket.OnMessage = (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            ProcessMessage(message);
        };
        
        Debug.Log("Attempting WebSocket connection to: " + wsUrl);
        
        try
        {
            await socket.Connect();
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to connect: " + ex.Message);
            QueueOnMainThread(() => OnConnectionError.Invoke("Connection failed: " + ex.Message));
        }
    }
    
    private void ProcessMessage(string message)
    {
        try
        {
            WebSocketMessage wsMsg = JsonConvert.DeserializeObject<WebSocketMessage>(message);
            
            if (wsMsg == null || string.IsNullOrEmpty(wsMsg.type))
            {
                Debug.LogWarning("Received malformed message");
                return;
            }
            
            switch (wsMsg.type)
            {
                case "queue:status":
                    var queueStatus = JsonConvert.DeserializeObject<QueueStatus>(wsMsg.data);
                    QueueOnMainThread(() => OnQueueStatus.Invoke(queueStatus));
                    break;
                
                case "match:found":
                    var matchFound = JsonConvert.DeserializeObject<MatchFoundData>(wsMsg.data);
                    QueueOnMainThread(() => OnMatchFound.Invoke(matchFound));
                    break;
                
                case "match:start":
                    var matchStart = JsonConvert.DeserializeObject<MatchStartData>(wsMsg.data);
                    QueueOnMainThread(() => OnMatchStart.Invoke(matchStart));
                    break;
                
                case "game:snapshot":
                    var gameSnapshot = JsonConvert.DeserializeObject<GameState>(wsMsg.data);
                    QueueOnMainThread(() => OnGameSnapshot.Invoke(gameSnapshot));
                    break;
                
                case "game:end":
                    var gameEnd = JsonConvert.DeserializeObject<GameEndData>(wsMsg.data);
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
    
    public async void JoinQueue()
    {
        if (!isConnected || socket == null)
        {
            Debug.LogError("Not connected to server");
            return;
        }
        
        var message = new WebSocketMessage
        {
            type = "queue:join",
            data = "{}"
        };
        
        string json = JsonConvert.SerializeObject(message);
        await socket.SendText(json);
        
        Debug.Log("Joined matchmaking queue");
    }
    
    public async void LeaveQueue()
    {
        if (!isConnected || socket == null)
        {
            Debug.LogError("Not connected to server");
            return;
        }
        
        var message = new WebSocketMessage
        {
            type = "queue:leave",
            data = "{}"
        };
        
        string json = JsonConvert.SerializeObject(message);
        await socket.SendText(json);
        
        Debug.Log("Left matchmaking queue");
    }
    
    public async void MarkMatchReady(int matchId)
    {
        if (!isConnected || socket == null)
        {
            Debug.LogError("Not connected to server");
            return;
        }
        
        var data = new { matchId = matchId };
        var message = new WebSocketMessage
        {
            type = "match:ready",
            data = JsonConvert.SerializeObject(data)
        };
        
        string json = JsonConvert.SerializeObject(message);
        await socket.SendText(json);
        
        Debug.Log("Marked ready for match: " + matchId);
    }
    
    public async void SendGameInput(GameInputData input)
    {
        if (!isConnected || socket == null)
        {
            Debug.LogError("Not connected to server");
            return;
        }
        
        var message = new WebSocketMessage
        {
            type = "game:input",
            data = JsonConvert.SerializeObject(input)
        };
        
        string json = JsonConvert.SerializeObject(message);
        await socket.SendText(json);
    }
    
    public async void Disconnect()
    {
        if (socket != null && socket.State == WebSocketState.Open)
        {
            await socket.Close();
        }
        socket = null;
        isConnected = false;
    }
    
    public bool IsConnected()
    {
        return isConnected && socket != null && socket.State == WebSocketState.Open;
    }
    
    private void OnDestroy()
    {
        Disconnect();
    }
    
    private async void OnApplicationQuit()
    {
        if (socket != null && socket.State == WebSocketState.Open)
        {
            await socket.Close();
        }
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
