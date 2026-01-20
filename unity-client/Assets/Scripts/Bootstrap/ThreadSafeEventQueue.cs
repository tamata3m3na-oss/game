using System;
using System.Collections.Concurrent;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public sealed class ThreadSafeEventQueue : MonoBehaviour
{
    public static ThreadSafeEventQueue Instance { get; private set; }

    private readonly ConcurrentQueue<Action> queue = new ConcurrentQueue<Action>();

    [SerializeField] private int maxActionsPerFrame = 256;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!UnityMainThread.IsMainThread)
        {
            Debug.LogError("[ThreadSafeEventQueue] Awake() was not called on the main thread.");
        }
    }

    public static void Enqueue(Action action)
    {
        if (action == null) return;

        if (Instance == null)
        {
            if (UnityMainThread.IsMainThread)
            {
                action();
                return;
            }

            return;
        }

        Instance.queue.Enqueue(action);
    }

    private void Update()
    {
        int processed = 0;

        while (processed < maxActionsPerFrame && queue.TryDequeue(out var action))
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            processed++;
        }
    }
}
