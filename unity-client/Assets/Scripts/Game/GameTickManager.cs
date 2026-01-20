using System;
using UnityEngine;

/// <summary>
/// GameTickManager: تنسيق الـ ticks والتوقيت
/// المسؤوليات:
/// - تتبع آخر tick تمت معالجته
/// - حساب lag وتأخير الشبكة
/// </summary>
public class GameTickManager : MonoBehaviour
{
    private static GameTickManager instance;

    public static GameTickManager GetInstance(bool logIfMissing = true)
    {
        if (instance != null) return instance;

        if (!UnityMainThread.IsMainThread)
        {
            if (logIfMissing)
            {
                Debug.LogWarning("[GameTickManager] GetInstance() called off the main thread.");
            }

            return null;
        }

        instance = FindObjectOfType<GameTickManager>();

        if (instance == null && logIfMissing)
        {
            Debug.LogError("[GameTickManager] GameTickManager is missing (Bootstrap failure).");
        }

        return instance;
    }

    private int lastProcessedTick = -1;
    private long lastProcessedTimestamp;

    private float averageNetworkDelay;
    private float maxNetworkDelay;
    private int snapshotCount;

    [SerializeField] private float delaySmoothingFactor = 0.1f;

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

    public void UpdateServerTick(int tick, long timestamp)
    {
        if (tick < lastProcessedTick)
        {
            Debug.LogWarning($"[GameTickManager] Received out-of-order tick: {tick} (last: {lastProcessedTick})");
            return;
        }

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        float networkDelay = (currentTime - timestamp) / 1000f;

        if (snapshotCount == 0)
        {
            averageNetworkDelay = networkDelay;
        }
        else
        {
            averageNetworkDelay = Mathf.Lerp(averageNetworkDelay, networkDelay, delaySmoothingFactor);
        }

        maxNetworkDelay = Mathf.Max(maxNetworkDelay, networkDelay);
        snapshotCount++;

        lastProcessedTick = tick;
        lastProcessedTimestamp = timestamp;
    }

    public int GetLastProcessedTick() => lastProcessedTick;

    public float GetNetworkDelay() => averageNetworkDelay;

    public float GetMaxNetworkDelay() => maxNetworkDelay;

    public float CalculateClockDrift(long serverTimestamp)
    {
        long localTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return (localTime - serverTimestamp) / 1000f;
    }

    public bool IsLagDetected() => averageNetworkDelay > 0.2f;

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
