using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// SnapshotProcessor: معالجة بيانات اللعبة من السيرفر
/// المسؤوليات:
/// - استقبال snapshots من NetworkEventManager
/// - التحقق من صحة البيانات الواردة
/// - تحديث GameStateRepository
/// </summary>
public class SnapshotProcessor : MonoBehaviour
{
    private static SnapshotProcessor instance;

    public static SnapshotProcessor GetInstance(bool logIfMissing = true)
    {
        if (instance != null) return instance;

        if (!UnityMainThread.IsMainThread)
        {
            if (logIfMissing)
            {
                Debug.LogWarning("[SnapshotProcessor] GetInstance() called off the main thread.");
            }

            return null;
        }

        instance = FindObjectOfType<SnapshotProcessor>();

        if (instance == null && logIfMissing)
        {
            Debug.LogError("[SnapshotProcessor] SnapshotProcessor is missing (Bootstrap failure).");
        }

        return instance;
    }

    private GameStateRepository stateRepository;
    private GameTickManager tickManager;
    private NetworkEventManager eventManager;

    private bool isSubscribed;
    private Coroutine waitCoroutine;

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
        waitCoroutine = StartCoroutine(WaitForManagers());
    }

    private IEnumerator WaitForManagers()
    {
        if (!ManagerInitializer.IsReady)
        {
            yield return ManagerInitializer.WaitForReady();
        }

        float start = Time.realtimeSinceStartup;
        const float timeoutSeconds = 10f;

        while (Time.realtimeSinceStartup - start < timeoutSeconds)
        {
            stateRepository ??= GameStateRepository.GetInstance(false);
            tickManager ??= GameTickManager.GetInstance(false);
            eventManager ??= NetworkEventManager.GetInstance(false);

            if (!isSubscribed && eventManager != null)
            {
                eventManager.OnGameSnapshotReceived += HandleGameSnapshot;
                eventManager.OnGameEndReceived += HandleGameEnd;
                isSubscribed = true;
            }

            if (stateRepository != null && tickManager != null && eventManager != null)
            {
                yield break;
            }

            yield return null;
        }

        Debug.LogError(
            "[SnapshotProcessor] Timed out waiting for managers. " +
            $"GameStateRepository={(stateRepository != null)} " +
            $"GameTickManager={(tickManager != null)} " +
            $"NetworkEventManager={(eventManager != null)}");
    }

    private void OnDestroy()
    {
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        if (isSubscribed && eventManager != null)
        {
            eventManager.OnGameSnapshotReceived -= HandleGameSnapshot;
            eventManager.OnGameEndReceived -= HandleGameEnd;
            isSubscribed = false;
        }

        if (instance == this)
        {
            instance = null;
        }
    }

    private void HandleGameSnapshot(NetworkGameState snapshotData)
    {
        if (snapshotData == null)
        {
            Debug.LogError("[SnapshotProcessor] Received null snapshot data");
            return;
        }

        stateRepository ??= GameStateRepository.GetInstance();
        tickManager ??= GameTickManager.GetInstance();

        if (stateRepository == null)
        {
            Debug.LogError("[SnapshotProcessor] GameStateRepository is missing (Bootstrap failure).");
            return;
        }

        if (tickManager == null)
        {
            Debug.LogError("[SnapshotProcessor] GameTickManager is missing (Bootstrap failure).");
            return;
        }

        try
        {
            if (!ValidateSnapshot(snapshotData))
            {
                Debug.LogWarning($"[SnapshotProcessor] Invalid snapshot rejected for tick {snapshotData.tick}");
                return;
            }

            // تأكد من إنشاء الطائرات والـ UI فوراً
            EnsureGameObjectsExist(snapshotData);

            tickManager.UpdateServerTick(snapshotData.tick, snapshotData.timestamp);
            stateRepository.UpdateGameState(snapshotData);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SnapshotProcessor] Error processing snapshot: {e.Message}");
        }
    }

    private void HandleGameEnd(GameEndData endData)
    {
        if (endData == null)
        {
            Debug.LogError("[SnapshotProcessor] Received null game end data");
            return;
        }

        Debug.Log($"[SnapshotProcessor] Game ended. Winner: {endData.winner}");

        stateRepository ??= GameStateRepository.GetInstance(false);
        if (endData.finalState != null && stateRepository != null)
        {
            stateRepository.UpdateGameState(endData.finalState);
        }
    }

    private bool ValidateSnapshot(NetworkGameState snapshot)
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

        return ValidatePlayerData(snapshot.player1) && ValidatePlayerData(snapshot.player2);
    }

    private bool ValidatePlayerData(PlayerStateData playerData)
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
    /// التأكد من إنشاء الطائرات والـ GameObjects فوراً عند أول snapshot
    /// </summary>
    private void EnsureGameObjectsExist(NetworkGameState snapshotData)
    {
        try
        {
            // البحث عن GameStateManager في المشهد الحالي
            var gameStateManager = FindObjectOfType<GameStateManager>();
            
            if (gameStateManager == null)
            {
                Debug.LogWarning("[SnapshotProcessor] GameStateManager not found in scene");
                return;
            }

            // التأكد من إنشاء الطائرات إذا لم تكن موجودة
            if (gameStateManager.GetPlayerShip() == null || gameStateManager.GetOpponentShip() == null)
            {
                Debug.Log("[SnapshotProcessor] Initializing ships from snapshot data");
                
                // استدعاء الدالة الجديدة مباشرة
                gameStateManager.InitializeShipsFromSnapshot(snapshotData);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SnapshotProcessor] Error ensuring game objects exist: {e.Message}");
        }
    }
}
