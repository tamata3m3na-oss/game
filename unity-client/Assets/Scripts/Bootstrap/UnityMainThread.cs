using System.Threading;
using UnityEngine;

public static class UnityMainThread
{
    private static int mainThreadId;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CaptureMainThread()
    {
        mainThreadId = Thread.CurrentThread.ManagedThreadId;
    }

    public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == mainThreadId;
}
