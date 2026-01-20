using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(-9000)]
public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    public string ServerUrl = "ws://localhost:3000";
    public string PvpNamespace = "/pvp";

    private ClientWebSocket socket;
    private CancellationTokenSource cancellationTokenSource;
    private Task connectTask;

    private NetworkEventManager eventManager;

    private bool isConnected;
    private string authToken = "";
    private Queue<Action> eventQueue = new Queue<Action>();

    public UnityEvent<string> OnConnected = new UnityEvent<string>();
    public UnityEvent<string> OnDisconnected = new UnityEvent<string>();
    public UnityEvent<string> OnConnectionError = new UnityEvent<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        lock (eventQueue)
        {
            while (eventQueue.Count > 0)
            {
                var action = eventQueue.Dequeue();
                action?.Invoke();
            }
        }
    }

    private void Enqueue(Action action)
    {
        lock (eventQueue)
        {
            eventQueue.Enqueue(action);
        }
    }

    public async Task InitializeAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            Debug.LogError("[NetworkManager] Initialize called with empty token.");
            return;
        }

        authToken = token;

        if (connectTask != null && !connectTask.IsCompleted)
        {
            return;
        }

        connectTask = ConnectAsync();
        await connectTask;
    }

    public void Initialize(string token)
    {
        _ = InitializeAsync(token);
    }

    private async Task ConnectAsync()
    {
        string wsUrl = ServerUrl + PvpNamespace + "?token=" + authToken;

        try
        {
            await DisconnectAsync().ConfigureAwait(false);

            socket = new ClientWebSocket();
            cancellationTokenSource = new CancellationTokenSource();

            Enqueue(() => Debug.Log("[NetworkManager] Connecting to: " + wsUrl));

            await socket.ConnectAsync(new Uri(wsUrl), cancellationTokenSource.Token).ConfigureAwait(false);

            isConnected = true;
            Enqueue(() =>
            {
                OnConnected.Invoke("Connected to server");
                Debug.Log("[NetworkManager] WebSocket connected");
                StartCoroutine(ReceiveLoop());
            });
        }
        catch (OperationCanceledException)
        {
            // expected on shutdown
        }
        catch (Exception ex)
        {
            isConnected = false;
            Enqueue(() =>
            {
                Debug.LogError("[NetworkManager] Failed to connect: " + ex.Message);
                OnConnectionError.Invoke("Connection failed: " + ex.Message);
            });
        }
    }

    private IEnumerator ReceiveLoop()
    {
        var buffer = new byte[4096];

        while (isConnected && socket != null && socket.State == WebSocketState.Open)
        {
            Task<WebSocketReceiveResult> receiveTask = null;
            try
            {
                receiveTask = socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                isConnected = false;
                ThreadSafeEventQueue.Enqueue(() =>
                {
                    Debug.LogError("[NetworkManager] WebSocket receive error: " + ex.Message);
                    OnConnectionError.Invoke("Connection error: " + ex.Message);
                });
                yield break;
            }

            while (!receiveTask.IsCompleted)
            {
                yield return null;
            }

            if (receiveTask.IsFaulted || receiveTask.IsCanceled)
            {
                isConnected = false;
                Exception ex = receiveTask.Exception?.InnerException ?? new Exception("WebSocket receive failed");
                ThreadSafeEventQueue.Enqueue(() =>
                {
                    Debug.LogError("[NetworkManager] WebSocket receive error: " + ex.Message);
                    OnConnectionError.Invoke("Connection error: " + ex.Message);
                });
                yield break;
            }

            WebSocketReceiveResult result = receiveTask.Result;

            if (result.MessageType == WebSocketMessageType.Close)
            {
                var closeTask = socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationTokenSource.Token);
                while (!closeTask.IsCompleted) yield return null;

                isConnected = false;
                ThreadSafeEventQueue.Enqueue(() =>
                {
                    OnDisconnected.Invoke("Server closed connection");
                    Debug.Log("[NetworkManager] WebSocket disconnected: server closed connection");
                });
                yield break;
            }

            if (result.MessageType == WebSocketMessageType.Text)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                ThreadSafeEventQueue.Enqueue(() => ProcessMessage(message));
            }
        }
    }

    private void ProcessMessage(string message)
    {
        if (eventManager == null)
        {
            eventManager = NetworkEventManager.Instance;
        }

        if (eventManager == null)
        {
            Debug.LogError("[NetworkManager] NetworkEventManager is missing (Bootstrap failure).");
            return;
        }

        try
        {
            WebSocketMessageWrapper wsMsg = JsonUtility.FromJson<WebSocketMessageWrapper>(message);

            if (wsMsg == null || string.IsNullOrEmpty(wsMsg.type))
            {
                Debug.LogWarning("[NetworkManager] Received malformed message");
                return;
            }

            switch (wsMsg.type)
            {
                case "queue:status":
                    eventManager.ProcessNetworkMessage(NetworkEventType.QueueStatus, wsMsg.data);
                    break;

                case "match:found":
                    eventManager.ProcessNetworkMessage(NetworkEventType.MatchFound, wsMsg.data);
                    break;

                case "match:start":
                    eventManager.ProcessNetworkMessage(NetworkEventType.MatchStart, wsMsg.data);
                    break;

                case "game:snapshot":
                    eventManager.ProcessNetworkMessage(NetworkEventType.GameSnapshot, wsMsg.data);
                    break;

                case "game:end":
                    eventManager.ProcessNetworkMessage(NetworkEventType.GameEnd, wsMsg.data);
                    break;

                default:
                    Debug.LogWarning("[NetworkManager] Unknown message type: " + wsMsg.type);
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[NetworkManager] Error processing message: " + ex.Message);
        }
    }

    private async Task SendMessageAsync(string json)
    {
        if (!IsConnected())
        {
            Enqueue(() => Debug.LogError("[NetworkManager] Not connected to server"));
            return;
        }

        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationTokenSource.Token)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // expected
        }
        catch (Exception ex)
        {
            Enqueue(() => Debug.LogError("[NetworkManager] Failed to send message: " + ex.Message));
        }
    }

    public void JoinQueue()
    {
        var message = new WebSocketMessageWrapper("queue:join", "{}");
        string json = JsonUtility.ToJson(message);
        _ = SendMessageAsync(json);
    }

    public void LeaveQueue()
    {
        var message = new WebSocketMessageWrapper("queue:leave", "{}");
        string json = JsonUtility.ToJson(message);
        _ = SendMessageAsync(json);
    }

    public void MarkMatchReady(int matchId)
    {
        var data = new MatchReadyData { matchId = matchId };
        var message = new WebSocketMessageWrapper("match:ready", JsonUtility.ToJson(data));
        string json = JsonUtility.ToJson(message);
        _ = SendMessageAsync(json);
    }

    public void SendGameInput(GameInputData input)
    {
        var message = new WebSocketMessageWrapper("game:input", JsonUtility.ToJson(input));
        string json = JsonUtility.ToJson(message);
        _ = SendMessageAsync(json);
    }

    public void Disconnect()
    {
        _ = DisconnectAsync();
    }

    private async Task DisconnectAsync()
    {
        cancellationTokenSource?.Cancel();

        if (socket != null && socket.State == WebSocketState.Open)
        {
            try
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None)
                    .ConfigureAwait(false);
            }
            catch
            {
                // ignore
            }
        }

        socket?.Dispose();
        socket = null;

        cancellationTokenSource?.Dispose();
        cancellationTokenSource = null;

        isConnected = false;
    }

    public bool IsConnected()
    {
        return isConnected && socket != null && socket.State == WebSocketState.Open;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        Disconnect();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }
}
