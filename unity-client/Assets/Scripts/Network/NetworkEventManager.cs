using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// مدير الأحداث الشبكية: وحدة التنسيق المركزية لجميع أحداث الشبكة
/// يحل محل نظام الـ event queue القديم ويوفر:
/// - نقطة تسجيل واضحة لكل حدث
/// - logging شامل لكل رسالة
/// - تسلسل واضح لمعالجة الأحداث
/// </summary>
public class NetworkEventManager : MonoBehaviour
{
    private static NetworkEventManager instance;
    public static NetworkEventManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<NetworkEventManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("NetworkEventManager");
                    instance = go.AddComponent<NetworkEventManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    #region Events

    public event Action<QueueStatusData> OnQueueStatusReceived;
    public event Action<MatchFoundData> OnMatchFoundReceived;
    public event Action<MatchStartData> OnMatchStartReceived;
    public event Action<NetworkGameStateData> OnGameSnapshotReceived;
    public event Action<GameEndData> OnGameEndReceived;

    #endregion

    #region Data Classes

    [Serializable]
    public class QueueStatusData
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
    public class MatchStartData
    {
        public int matchId;
        public OpponentData opponent;
        public string color;
    }

    [Serializable]
    public class OpponentData
    {
        public int id;
        public string username;
        public int rating;
    }

    [Serializable]
    public class NetworkGameStateData
    {
        public int matchId;
        public PlayerStateData player1;
        public PlayerStateData player2;
        public int tick;
        public long timestamp;
        public int winner;
        public string status;
    }

    [Serializable]
    public class PlayerStateData
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
        public NetworkGameStateData finalState;
    }

    #endregion

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

    /// <summary>
    /// معالجة الرسائل الواردة من الشبكة وفقاً لنوعها
    /// </summary>
    public void ProcessNetworkMessage(NetworkEventType eventType, string jsonData)
    {
        try
        {
            Debug.Log($"[NetworkEventManager] Processing {eventType} event");

            switch (eventType)
            {
                case NetworkEventType.QueueStatus:
                    var queueStatus = JsonUtility.FromJson<QueueStatusData>(jsonData);
                    if (queueStatus != null)
                    {
                        Debug.Log($"[NetworkEventManager] Queue status: position {queueStatus.position}");
                        OnQueueStatusReceived?.Invoke(queueStatus);
                    }
                    break;

                case NetworkEventType.MatchFound:
                    var matchFound = JsonUtility.FromJson<MatchFoundData>(jsonData);
                    if (matchFound != null)
                    {
                        Debug.Log($"[NetworkEventManager] Match found: {matchFound.matchId}");
                        OnMatchFoundReceived?.Invoke(matchFound);
                    }
                    break;

                case NetworkEventType.MatchStart:
                    var matchStart = JsonUtility.FromJson<MatchStartData>(jsonData);
                    if (matchStart != null)
                    {
                        Debug.Log($"[NetworkEventManager] Match start: {matchStart.matchId}");
                        OnMatchStartReceived?.Invoke(matchStart);
                    }
                    break;

                case NetworkEventType.GameSnapshot:
                    var gameSnapshot = JsonUtility.FromJson<NetworkGameStateData>(jsonData);
                    if (gameSnapshot != null)
                    {
                        Debug.Log($"[NetworkEventManager] Game snapshot: tick {gameSnapshot.tick}");
                        OnGameSnapshotReceived?.Invoke(gameSnapshot);
                    }
                    break;

                case NetworkEventType.GameEnd:
                    var gameEnd = JsonUtility.FromJson<GameEndData>(jsonData);
                    if (gameEnd != null)
                    {
                        Debug.Log($"[NetworkEventManager] Game end: winner {gameEnd.winner}");
                        OnGameEndReceived?.Invoke(gameEnd);
                    }
                    break;

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

    /// <summary>
    /// تسجيل الاستماع لأنواع أحداث محددة
    /// </summary>
    public void RegisterEventListener(NetworkEventType eventType, Delegate listener)
    {
        Debug.Log($"[NetworkEventManager] Registering listener for {eventType}");
    }

    /// <summary>
    /// إلغاء تسجيل الاستماع
    /// </summary>
    public void UnregisterEventListener(NetworkEventType eventType, Delegate listener)
    {
        Debug.Log($"[NetworkEventManager] Unregistering listener for {eventType}");
    }
}