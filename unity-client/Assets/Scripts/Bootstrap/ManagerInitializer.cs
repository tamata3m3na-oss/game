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
        return UnityMainThread.IsMainThread
            && ThreadSafeEventQueue.Instance != null
            && NetworkEventManager.GetInstance(false) != null
            && NetworkManager.Instance != null
            && AuthManager.Instance != null
            && InputController.Instance != null
            && GameStateRepository.GetInstance(false) != null
            && GameTickManager.GetInstance(false) != null
            && SnapshotProcessor.GetInstance(false) != null
            && AnimationController.Instance != null
            && ParticleController.Instance != null
            && TransitionManager.Instance != null
            && FindObjectOfType<SceneInitializer>() != null;
    }

    private void LogMissingManagers()
    {
        Debug.LogError("[ManagerInitializer] Timed out while waiting for managers. Missing:" +
                       $" ThreadSafeEventQueue={(ThreadSafeEventQueue.Instance != null)}" +
                       $" NetworkEventManager={(NetworkEventManager.GetInstance(false) != null)}" +
                       $" NetworkManager={(NetworkManager.Instance != null)}" +
                       $" AuthManager={(AuthManager.Instance != null)}" +
                       $" InputController={(InputController.Instance != null)}" +
                       $" GameStateRepository={(GameStateRepository.GetInstance(false) != null)}" +
                       $" GameTickManager={(GameTickManager.GetInstance(false) != null)}" +
                       $" SnapshotProcessor={(SnapshotProcessor.GetInstance(false) != null)}" +
                       $" AnimationController={(AnimationController.Instance != null)}" +
                       $" ParticleController={(ParticleController.Instance != null)}" +
                       $" TransitionManager={(TransitionManager.Instance != null)}" +
                       $" SceneInitializer={(FindObjectOfType<SceneInitializer>() != null)}");
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
