using UnityEngine;

/// <summary>
GameTickManager: تنسيق الـ ticks والتوقيت
المسؤوليات:
- تتبع آخر tick تمت معالجته
- حساب lag وتأخير الشبكة
- التنسيق بين الـ snapshots والـ ticks
</summary>
public class GameTickManager : MonoBehaviour
{
    private static GameTickManager instance;
    public static GameTickManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameTickManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameTickManager");
                    instance = go.AddComponent<GameTickManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    private int lastProcessedTick = -1;
    private long lastProcessedTimestamp = 0;

    // إحصائيات الشبكة
    private float averageNetworkDelay = 0f;
    private float maxNetworkDelay = 0f;
    private int snapshotCount = 0;

    // Smoothing
    private float delaySmoothingFactor = 0.1f;

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
    /// تحديث التيك الحالي من السيرفر
    /// </summary>
    public void UpdateServerTick(int tick, long timestamp)
    {
        if (tick < lastProcessedTick)
        {
            Debug.LogWarning($"[GameTickManager] Received out-of-order tick: {tick} (last: {lastProcessedTick})");
            return;
        }

        // حساب تأخير الشبكة
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        float networkDelay = (currentTime - timestamp) / 1000f; // convert to seconds

        // تحديث المتوسط المتحرك للتأخير
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

        // تحديث آخر تيك
        lastProcessedTick = tick;
        lastProcessedTimestamp = timestamp;

        Debug.Log($"[GameTickManager] Updated tick {tick}, Network delay: {networkDelay:F3}s (avg: {averageNetworkDelay:F3}s)");
    }

    /// <summary>
    /// الحصول على آخر tick تمت معالجته
    /// </summary>
    public int GetLastProcessedTick()
    {
        return lastProcessedTick;
    }

    /// <summary>
    /// الحصول على تأخير الشبكة الحالي
    /// </summary>
    public float GetNetworkDelay()
    {
        return averageNetworkDelay;
    }

    /// <summary>
    /// الحصول على أقصى تأخير تم رصده
    /// </summary>
    public float GetMaxNetworkDelay()
    {
        return maxNetworkDelay;
    }

    /// <summary>
    /// حساب التباين بين التوقيت المحلي والسيرفر
    /// </summary>
    public float CalculateClockDrift(long serverTimestamp)
    {
        long localTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return (localTime - serverTimestamp) / 1000f;
    }

    /// <summary>
    /// التحقق من حالة الـ lag
    /// </summary>
    public bool IsLagDetected()
    {
        return averageNetworkDelay > 0.2f; // أكثر من 200ms يعتبر lag
    }
}