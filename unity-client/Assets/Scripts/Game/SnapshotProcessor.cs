using UnityEngine;
using System;

/// <summary>
SnapshotProcessor: معالجة بيانات اللعبة من السيرفر
المسؤوليات:
- استقبال snapshots من NetworkEventManager
- التحقق من صحة البيانات الواردة
- تحديث GameStateRepository
- إرسال أحداث التغيير للطبقات الأخرى
</summary>
public class SnapshotProcessor : MonoBehaviour
{
    private static SnapshotProcessor instance;
    public static SnapshotProcessor Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SnapshotProcessor>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SnapshotProcessor");
                    instance = go.AddComponent<SnapshotProcessor>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    private GameStateRepository stateRepository;
    private GameTickManager tickManager;

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

    private void Start()
    {
        stateRepository = GameStateRepository.Instance;
        tickManager = GameTickManager.Instance;

        // التسجيل لاستلام أحداث الـ snapshots
        NetworkEventManager.Instance.OnGameSnapshotReceived += HandleGameSnapshot;
        NetworkEventManager.Instance.OnGameEndReceived += HandleGameEnd;
    }

    private void OnDestroy()
    {
        if (NetworkEventManager.Instance != null)
        {
            NetworkEventManager.Instance.OnGameSnapshotReceived -= HandleGameSnapshot;
            NetworkEventManager.Instance.OnGameEndReceived -= HandleGameEnd;
        }
    }

    /// <summary>
    /// معالجة snapshot جديد من السيرفر
    /// </summary>
    private void HandleGameSnapshot(NetworkEventManager.NetworkGameStateData snapshotData)
    {
        if (snapshotData == null)
        {
            Debug.LogError("[SnapshotProcessor] Received null snapshot data");
            return;
        }

        try
        {
            // التحقق من صحة البيانات
            if (!ValidateSnapshot(snapshotData))
            {
                Debug.LogWarning($"[SnapshotProcessor] Invalid snapshot rejected for tick {snapshotData.tick}");
                return;
            }

            // تحديث التيك manager
            tickManager.UpdateServerTick(snapshotData.tick, snapshotData.timestamp);

            // تحويل البيانات إلى الصيغة المطلوبة للـ Repository
            var gameState = ConvertToRepositoryFormat(snapshotData);

            // تحديث الـ Repository (Single Source of Truth)
            stateRepository.UpdateGameState(gameState);

            Debug.Log($"[SnapshotProcessor] Processed snapshot tick {snapshotData.tick} successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SnapshotProcessor] Error processing snapshot: {e.Message}");
        }
    }

    /// <summary>
    /// معالجة نهاية اللعبة
    /// </summary>
    private void HandleGameEnd(NetworkEventManager.GameEndData endData)
    {
        if (endData == null)
        {
            Debug.LogError("[SnapshotProcessor] Received null game end data");
            return;
        }

        Debug.Log($"[SnapshotProcessor] Game ended. Winner: {endData.winner}");

        // يمكن إضافة منطق إضافي هنا
        // مثل: حفض إحصائيات اللعبة، تنظيم الموارد، إلخ
    }

    /// <summary>
    /// التحقق من صحة بيانات الـ snapshot
    /// </summary>
    private bool ValidateSnapshot(NetworkEventManager.NetworkGameStateData snapshot)
    {
        if (snapshot.player1 == null || snapshot.player2 == null)
        {
            Debug.LogWarning("[SnapshotProcessor] Snapshot missing player data");
            return false;
        }

        if (snapshot.player1.id <= 0 || snapshot.player2.id <= 0)
        {
            Debug.LogWarning("[SnapshotProcessor] Invalid player IDs in snapshot");
            return false;
        }

        if (snapshot.tick < 0)
        {
            Debug.LogWarning("[SnapshotProcessor] Invalid tick number");
            return false;
        }

        // التحقق من صحة بيانات اللاعبين
        if (!ValidatePlayerData(snapshot.player1) || !ValidatePlayerData(snapshot.player2))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// التحقق من صحة بيانات لاعب واحد
    /// </summary>
    private bool ValidatePlayerData(NetworkEventManager.PlayerStateData playerData)
    {
        if (playerData.health < 0 || playerData.health > 100)
        {
            Debug.LogWarning($"[SnapshotProcessor] Invalid health value: {playerData.health}");
            return false;
        }

        if (playerData.shieldHealth < 0 || playerData.shieldHealth > 50)
        {
            Debug.LogWarning($"[SnapshotProcessor] Invalid shield health: {playerData.shieldHealth}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// تحويل بيانات الـ NetworkEventManager إلى صيغة الـ Repository
    /// </summary>
    private NetworkGameState ConvertToRepositoryFormat(NetworkEventManager.NetworkGameStateData eventData)
    {
        var repositoryState = new NetworkGameState
        {
            matchId = eventData.matchId,
            tick = eventData.tick,
            timestamp = eventData.timestamp,
            winner = eventData.winner,
            status = eventData.status
        };

        if (eventData.player1 != null)
        {
            repositoryState.player1 = ConvertPlayerData(eventData.player1);
        }

        if (eventData.player2 != null)
        {
            repositoryState.player2 = ConvertPlayerData(eventData.player2);
        }

        return repositoryState;
    }

    /// <summary>
    /// تحويل بيانات اللاعب
    /// </summary>
    private PlayerStateData ConvertPlayerData(NetworkEventManager.PlayerStateData eventPlayerData)
    {
        return new PlayerStateData
        {
            id = eventPlayerData.id,
            x = eventPlayerData.x,
            y = eventPlayerData.y,
            rotation = eventPlayerData.rotation,
            health = eventPlayerData.health,
            shieldHealth = eventPlayerData.shieldHealth,
            shieldActive = eventPlayerData.shieldActive,
            shieldEndTick = eventPlayerData.shieldEndTick,
            fireReady = eventPlayerData.fireReady,
            fireReadyTick = eventPlayerData.fireReadyTick,
            abilityReady = eventPlayerData.abilityReady,
            lastAbilityTime = eventPlayerData.lastAbilityTime,
            damageDealt = eventPlayerData.damageDealt
        };
    }
}