using UnityEngine;
using UI.Animations;
using System.Collections;
using System.Linq;

public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateBootstrapObject()
    {
        // Try Enhanced Bootstrap first, fallback to standard
        if (Object.FindObjectOfType<BootstrapRunnerEnhanced>() != null)
        {
            return;
        }

        if (Object.FindObjectOfType<BootstrapRunner>() != null)
        {
            return;
        }

        var go = new GameObject("_Bootstrap");
        Object.DontDestroyOnLoad(go);
        
        // Use enhanced bootstrap if available, otherwise standard
        go.AddComponent<BootstrapRunnerEnhanced>();
    }
}

[DefaultExecutionOrder(-100)]
public sealed class BootstrapRunner : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        Debug.Log("[BootstrapRunner] Starting manager initialization...");

        // ManagerInitializer should exist before any other manager relies on singleton access.
        EnsureSingletonComponent<ManagerInitializer>();

        // Core threading + network
        Debug.Log("[BootstrapRunner] Initializing core managers...");
        EnsureSingletonComponent<ThreadSafeEventQueue>();
        EnsureSingletonComponent<NetworkEventManager>();
        EnsureSingletonComponent<NetworkManager>();

        // Auth + input
        Debug.Log("[BootstrapRunner] Initializing auth and input managers...");
        EnsureSingletonComponent<AuthManager>();
        EnsureSingletonComponent<InputController>();

        // Game state processing
        Debug.Log("[BootstrapRunner] Initializing game state managers...");
        EnsureSingletonComponent<GameStateRepository>();
        EnsureSingletonComponent<GameTickManager>();
        EnsureSingletonComponent<SnapshotProcessor>();

        // UI helpers - ParticleController first due to execution order
        Debug.Log("[BootstrapRunner] Initializing UI managers...");
        EnsureSingletonComponent<ParticleController>();
        EnsureSingletonComponent<AnimationController>();
        EnsureSingletonComponent<TransitionManager>();

        // Scene hooks
        Debug.Log("[BootstrapRunner] Initializing scene managers...");
        EnsureSingletonComponent<SceneInitializer>();

        // Verify all components with detailed logging
        Debug.Log("[BootstrapRunner] Verifying manager initialization...");
        bool allVerified = true;
        allVerified &= VerifySingletonComponent<ThreadSafeEventQueue>();
        allVerified &= VerifySingletonComponent<NetworkEventManager>();
        allVerified &= VerifySingletonComponent<NetworkManager>();
        allVerified &= VerifySingletonComponent<AuthManager>();
        allVerified &= VerifySingletonComponent<InputController>();
        allVerified &= VerifySingletonComponent<GameStateRepository>();
        allVerified &= VerifySingletonComponent<GameTickManager>();
        allVerified &= VerifySingletonComponent<SnapshotProcessor>();
        allVerified &= VerifySingletonComponent<ParticleController>();
        allVerified &= VerifySingletonComponent<AnimationController>();
        allVerified &= VerifySingletonComponent<TransitionManager>();
        allVerified &= VerifySingletonComponent<SceneInitializer>();

        // Perform comprehensive safety check
        bool safetyCheckPassed = ManagersSafetyCheck.CheckAllManagers("Bootstrap");

        if (allVerified && safetyCheckPassed)
        {
            Debug.Log("[BootstrapRunner] All managers initialized and verified successfully.");
            ManagerInitializer.Instance?.NotifyBootstrapCompleted();
        }
        else
        {
            Debug.LogError("[BootstrapRunner] Manager initialization failed. Some managers may not be ready.");
        }
    }

    private void EnsureSingletonComponent<T>() where T : Component
    {
        if (FindObjectOfType<T>() != null)
        {
            Debug.Log($"[BootstrapRunner] {typeof(T).Name} already exists, skipping creation.");
            return;
        }

        try
        {
            Component component = gameObject.AddComponent<T>();
            Debug.Log($"[BootstrapRunner] Successfully created {typeof(T).Name}");
            
            if (component == null)
            {
                Debug.LogError($"[BootstrapRunner] Failed to create {typeof(T).Name} - AddComponent returned null");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[BootstrapRunner] Exception creating {typeof(T).Name}: {ex.Message}");
        }
    }

    private bool VerifySingletonComponent<T>() where T : Component
    {
        bool isInitialized = FindObjectOfType<T>() != null;
        
        if (isInitialized)
        {
            Debug.Log($"[BootstrapRunner] ✅ {typeof(T).Name} verified successfully.");
        }
        else
        {
            Debug.LogError($"[BootstrapRunner] ❌ Failed to initialize {typeof(T).Name}.");
        }
        
        return isInitialized;
    }
}
