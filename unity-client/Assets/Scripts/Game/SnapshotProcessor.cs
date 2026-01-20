using System;
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

    public static SnapshotProcessor Instance
    {
        get
        {
            if (instance != null) return instance;

            if (!UnityMainThread.IsMainThread)
            {
                return null;
            }

            instance = FindObjectOfType<SnapshotProcessor>();
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

        var nem = NetworkEventManager.Instance;
        if (nem != null)
        {
            nem.OnGameSnapshotReceived += HandleGameSnapshot;
            nem.OnGameEndReceived += HandleGameEnd;
        }
        else
        {
            Debug.LogError("[SnapshotProcessor] NetworkEventManager is missing (Bootstrap failure).");
        }
    }

    private void OnDestroy()
    {
        var nem = NetworkEventManager.Instance;
        if (nem != null)
        {
            nem.OnGameSnapshotReceived -= HandleGameSnapshot;
            nem.OnGameEndReceived -= HandleGameEnd;
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
}
