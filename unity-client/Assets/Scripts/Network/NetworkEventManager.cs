using System;
using UnityEngine;

public class NetworkEventManager : MonoBehaviour
{
    private static NetworkEventManager instance;

    public static NetworkEventManager GetInstance(bool logIfMissing = true)
    {
        if (instance != null) return instance;

        if (!UnityMainThread.IsMainThread)
        {
            if (logIfMissing)
            {
                Debug.LogWarning("[NetworkEventManager] GetInstance() called off the main thread.");
            }

            return null;
        }

        instance = FindObjectOfType<NetworkEventManager>();

        if (instance == null && logIfMissing)
        {
            Debug.LogError("[NetworkEventManager] NetworkEventManager is missing (Bootstrap failure).");
        }

        return instance;
    }

    public QueueStatusData LastQueueStatus { get; private set; }
    public MatchFoundData LastMatchFound { get; private set; }
    public MatchStartData LastMatchStart { get; private set; }
    public NetworkGameState LastSnapshot { get; private set; }
    public GameEndData LastGameEnd { get; private set; }

    public event Action<QueueStatusData> OnQueueStatusReceived;
    public event Action<MatchFoundData> OnMatchFoundReceived;
    public event Action<MatchStartData> OnMatchStartReceived;
    public event Action<NetworkGameState> OnGameSnapshotReceived;
    public event Action<GameEndData> OnGameEndReceived;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ProcessNetworkMessage(NetworkEventType eventType, string jsonData)
    {
        try
        {
            switch (eventType)
            {
                case NetworkEventType.QueueStatus:
                {
                    var payload = JsonUtility.FromJson<QueueStatusData>(jsonData);
                    LastQueueStatus = payload;
                    OnQueueStatusReceived?.Invoke(payload);
                    break;
                }

                case NetworkEventType.MatchFound:
                {
                    var payload = JsonUtility.FromJson<MatchFoundData>(jsonData);
                    LastMatchFound = payload;
                    OnMatchFoundReceived?.Invoke(payload);
                    break;
                }

                case NetworkEventType.MatchStart:
                {
                    var payload = JsonUtility.FromJson<MatchStartData>(jsonData);
                    LastMatchStart = payload;
                    OnMatchStartReceived?.Invoke(payload);
                    break;
                }

                case NetworkEventType.GameSnapshot:
                {
                    var payload = JsonUtility.FromJson<NetworkGameState>(jsonData);
                    LastSnapshot = payload;
                    OnGameSnapshotReceived?.Invoke(payload);
                    break;
                }

                case NetworkEventType.GameEnd:
                {
                    var payload = JsonUtility.FromJson<GameEndData>(jsonData);
                    LastGameEnd = payload;
                    OnGameEndReceived?.Invoke(payload);
                    break;
                }

                default:
                    Debug.LogWarning($"[NetworkEventManager] Unknown event type: {eventType}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetworkEventManager] Error processing {eventType}: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
