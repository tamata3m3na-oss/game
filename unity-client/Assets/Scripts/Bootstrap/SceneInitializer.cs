using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneInitializer : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        InitializeScene(SceneManager.GetActiveScene());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeScene(scene);
    }

    private void InitializeScene(Scene scene)
    {
        if (scene.name == "LobbyScene")
        {
            var auth = AuthManager.Instance;
            var network = NetworkManager.Instance;

            if (auth != null && network != null && auth.IsLoggedIn() && !network.IsConnected())
            {
                network.Initialize(auth.GetAuthToken());
            }
        }
        else if (scene.name == "GameScene")
        {
            var gsm = FindObjectOfType<GameStateManager>();
            if (gsm == null)
            {
                Debug.LogWarning("[SceneInitializer] GameScene loaded without a GameStateManager.");
            }
        }
    }
}
