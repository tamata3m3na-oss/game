using UnityEngine;
using UI.Animations;

public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
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

[DefaultExecutionOrder(-10002)]
public sealed class BootstrapRunner : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        EnsureSingletonComponent<ThreadSafeEventQueue>();
        EnsureSingletonComponent<NetworkEventManager>();
        EnsureSingletonComponent<NetworkManager>();
        EnsureSingletonComponent<AuthManager>();
        EnsureSingletonComponent<InputController>();

        EnsureSingletonComponent<GameStateRepository>();
        EnsureSingletonComponent<GameTickManager>();
        EnsureSingletonComponent<SnapshotProcessor>();

        EnsureSingletonComponent<AnimationController>();
        EnsureSingletonComponent<ParticleController>();
        EnsureSingletonComponent<TransitionManager>();

        EnsureSingletonComponent<SceneInitializer>();
    }

    private void EnsureSingletonComponent<T>() where T : Component
    {
        if (FindObjectOfType<T>() != null)
        {
            return;
        }

        gameObject.AddComponent<T>();
    }
}
