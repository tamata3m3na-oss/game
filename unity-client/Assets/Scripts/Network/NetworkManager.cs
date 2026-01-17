using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }
    
    public string ServerUrl = "http://localhost:3000";
    public string PvpNamespace = "/pvp";
    
    private SocketIOUnity socket;
    private Queue<Action> eventQueue = new Queue<Action>();
    private bool isConnected = false;
    private string authToken = "";
    
    // Events
    public UnityEvent<string> OnConnected = new UnityEvent<string>();
    public UnityEvent<string> OnDisconnected = new UnityEvent<string>();
    public UnityEvent<string> OnConnectionError = new UnityEvent<string>();
    
    // Queue Events
    public UnityEvent<QueueStatus> OnQueueStatus = new UnityEvent<QueueStatus>();
    
    // Match Events
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
        public string color; // "white" or "black"
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
        // Process queued events on main thread
        while (eventQueue.Count > 0)
        {
            var action = eventQueue.Dequeue();
            action.Invoke();
        }
    }
    
    public void Initialize(string token)
    {
        authToken = token;
        
        var uri = new Uri(ServerUrl);
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
            {
                {"token", authToken}
            },
            EIO = 4,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
            Reconnection = true,
            ReconnectionAttempts = 5,
            ReconnectionDelay = 1000,
            Timeout = 30000,
            Path = PvpNamespace
        });
        
        SetupSocketEvents();
        
        Debug.Log("Connecting to server...");
        socket.Connect();
    }
    
    private void SetupSocketEvents()
    {
        socket.OnConnected += (sender, e) =>
        {
            isConnected = true;
            QueueOnMainThread(() => OnConnected.Invoke("Connected to server"));
            Debug.Log("Connected to server");
        };
        
        socket.OnDisconnected += (sender, e) =>
        {
            isConnected = false;
            QueueOnMainThread(() => OnDisconnected.Invoke("Disconnected: " + e));
            Debug.Log("Disconnected: " + e);
        };
        
        socket.OnError += (sender, e) =>
        {
            QueueOnMainThread(() => OnConnectionError.Invoke("Error: " + e));
            Debug.LogError("Socket error: " + e);
        };
        
        // Queue events
        socket.On("queue:status", (response) =>
        {
            var status = response.GetValue<QueueStatus>();
            QueueOnMainThread(() => OnQueueStatus.Invoke(status));
        });
        
        // Match events
        socket.On("match:found", (response) =>
        {
            var data = response.GetValue<MatchFoundData>();
            QueueOnMainThread(() => OnMatchFound.Invoke(data));
        });
        
        socket.On("match:start", (response) =>
        {
            var data = response.GetValue<MatchStartData>();
            QueueOnMainThread(() => OnMatchStart.Invoke(data));
        });
        
        // Game events
        socket.On("game:snapshot", (response) =>
        {
            var state = response.GetValue<GameState>();
            QueueOnMainThread(() => OnGameSnapshot.Invoke(state));
        });
        
        socket.On("game:end", (response) =>
        {
            var data = response.GetValue<GameEndData>();
            QueueOnMainThread(() => OnGameEnd.Invoke(data));
        });
    }
    
    private void QueueOnMainThread(Action action)
    {
        lock (eventQueue)
        {
            eventQueue.Enqueue(action);
        }
    }
    
    public void JoinQueue()
    {
        if (!isConnected || socket == null)
        {
            Debug.LogError("Not connected to server");
            return;
        }
        
        socket.EmitAsync("queue:join");
        Debug.Log("Joined matchmaking queue");
    }
    
    public void LeaveQueue()
    {
        if (!isConnected || socket == null)
        {
            Debug.LogError("Not connected to server");
            return;
        }
        
        socket.EmitAsync("queue:leave");
        Debug.Log("Left matchmaking queue");
    }
    
    public void MarkMatchReady(int matchId)
    {
        if (!isConnected || socket == null)
        {
            Debug.LogError("Not connected to server");
            return;
        }
        
        var data = new { matchId = matchId };
        socket.EmitAsync("match:ready", data);
        Debug.Log("Marked ready for match: " + matchId);
    }
    
    public void SendGameInput(GameInputData input)
    {
        if (!isConnected || socket == null)
        {
            Debug.LogError("Not connected to server");
            return;
        }
        
        socket.EmitAsync("game:input", input);
    }
    
    public void Disconnect()
    {
        if (socket != null)
        {
            socket.Disconnect();
            socket = null;
        }
        isConnected = false;
    }
    
    public bool IsConnected()
    {
        return isConnected;
    }
    
    private void OnDestroy()
    {
        Disconnect();
    }
    
    private void OnApplicationQuit()
    {
        Disconnect();
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