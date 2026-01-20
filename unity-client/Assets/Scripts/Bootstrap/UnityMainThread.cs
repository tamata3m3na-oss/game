using System.Threading;
using UnityEngine;

public static class UnityMainThread
{
    private static int mainThreadId;

    static UnityMainThread()
    {
        // Best-effort initialization in case RuntimeInitializeOnLoadMethod order is not guaranteed.
        mainThreadId = Thread.CurrentThread.ManagedThreadId;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CaptureMainThread()
    {
        mainThreadId = Thread.CurrentThread.ManagedThreadId;
    }

    public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == mainThreadId;
}
