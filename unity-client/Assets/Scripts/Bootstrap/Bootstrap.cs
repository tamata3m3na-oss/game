using UnityEngine;
using UI.Animations;

public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateBootstrapObject()
    {
        if (Object.FindObjectOfType<BootstrapRunner>() != null)
        {
            return;
        }

        var go = new GameObject("_Bootstrap");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<BootstrapRunner>();
    }
}

[DefaultExecutionOrder(-100)]
public sealed class BootstrapRunner : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // ManagerInitializer should exist before any other manager relies on singleton access.
        EnsureSingletonComponent<ManagerInitializer>();

        // Core threading + network
        EnsureSingletonComponent<ThreadSafeEventQueue>();
        EnsureSingletonComponent<NetworkEventManager>();
        EnsureSingletonComponent<NetworkManager>();

        // Auth + input
        EnsureSingletonComponent<AuthManager>();
        EnsureSingletonComponent<InputController>();

        // Game state processing
        EnsureSingletonComponent<GameStateRepository>();
        EnsureSingletonComponent<GameTickManager>();
        EnsureSingletonComponent<SnapshotProcessor>();

        // UI helpers
        EnsureSingletonComponent<AnimationController>();
        EnsureSingletonComponent<ParticleController>();
        EnsureSingletonComponent<TransitionManager>();

        // Scene hooks
        EnsureSingletonComponent<SceneInitializer>();

        VerifySingletonComponent<ThreadSafeEventQueue>();
        VerifySingletonComponent<NetworkEventManager>();
        VerifySingletonComponent<NetworkManager>();
        VerifySingletonComponent<AuthManager>();
        VerifySingletonComponent<InputController>();
        VerifySingletonComponent<GameStateRepository>();
        VerifySingletonComponent<GameTickManager>();
        VerifySingletonComponent<SnapshotProcessor>();
        VerifySingletonComponent<AnimationController>();
        VerifySingletonComponent<ParticleController>();
        VerifySingletonComponent<TransitionManager>();
        VerifySingletonComponent<SceneInitializer>();

        ManagerInitializer.Instance?.NotifyBootstrapCompleted();
    }

    private void EnsureSingletonComponent<T>() where T : Component
    {
        if (FindObjectOfType<T>() != null)
        {
            return;
        }

        gameObject.AddComponent<T>();
    }

    private void VerifySingletonComponent<T>() where T : Component
    {
        if (FindObjectOfType<T>() == null)
        {
            Debug.LogError($"[BootstrapRunner] Failed to initialize {typeof(T).Name}.");
        }
    }
}
