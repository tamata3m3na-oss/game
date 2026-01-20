using System.Collections;
using UnityEngine;
using UI.Animations;

/// <summary>
/// ManagerInitializer:
/// - Ensures the bootstrap object exists early
/// - Waits until all critical managers are created and initialized
/// - Provides a central "ready" flag for scripts that need to safely wait
/// </summary>
[DefaultExecutionOrder(-200)]
public sealed class ManagerInitializer : MonoBehaviour
{
    private static ManagerInitializer instance;

    public static ManagerInitializer Instance => instance;

    public static bool IsReady => instance != null && instance.isReady;

    [SerializeField] private float initializationTimeoutSeconds = 10f;

    private bool isReady;



    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        StartCoroutine(InitializeManagers());
    }

    public void NotifyBootstrapCompleted()
    {
        if (isReady) return;

        if (AreCriticalManagersReady())
        {
            isReady = true;
            Debug.Log("[ManagerInitializer] All managers are ready.");
        }
    }

    public static IEnumerator WaitForReady(float timeoutSeconds = 10f)
    {
        float start = Time.realtimeSinceStartup;

        while (!IsReady)
        {
            if (Time.realtimeSinceStartup - start >= timeoutSeconds)
            {
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator InitializeManagers()
    {
        EnsureBootstrapRunnerExists();

        float start = Time.realtimeSinceStartup;

        while (!AreCriticalManagersReady())
        {
            if (Time.realtimeSinceStartup - start >= initializationTimeoutSeconds)
            {
                LogMissingManagers();
                yield break;
            }

            yield return null;
        }

        if (!isReady)
        {
            isReady = true;
            Debug.Log("[ManagerInitializer] All managers are ready.");
        }
    }

    private void EnsureBootstrapRunnerExists()
    {
        if (FindObjectOfType<BootstrapRunner>() != null)
        {
            return;
        }

        var bootstrapGo = GameObject.Find("_Bootstrap") ?? new GameObject("_Bootstrap");
        Object.DontDestroyOnLoad(bootstrapGo);

        // Adding the runner will, in turn, add all singleton managers.
        bootstrapGo.AddComponent<BootstrapRunner>();
    }

    private bool AreCriticalManagersReady()
    {
        // Use the comprehensive safety check
        return ManagersSafetyCheck.CheckAllManagers("ManagerInitializer");
    }

    private void LogMissingManagers()
    {
        Debug.LogError("[ManagerInitializer] Timed out while waiting for managers.");
        
        // The ManagersSafetyCheck will provide detailed logging
        ManagersSafetyCheck.CheckAllManagers("ManagerInitializerTimeout");
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
