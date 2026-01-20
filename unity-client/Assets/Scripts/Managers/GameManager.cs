using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Scene-based manager coordinator.
/// 
/// IMPORTANT: All managers should be assigned via scene prefabs (LoginScene).
/// This class does NOT create GameObjects at runtime - it only finds and references
/// existing managers to avoid Unity lifecycle violations.
/// 
/// Removed Features:
/// - InitializeManagers() method (violated Unity lifecycle by using new GameObject())
/// - Dynamic AddComponent() calls (MonoManager may not be ready, causes NULL references)
/// - Runtime GameObject creation (breaks scene-based initialization)
/// 
/// Scene Requirements:
/// - AuthManager must exist in LoginScene as a prefab/GameObject
/// - NetworkManager must exist in LoginScene as a prefab/GameObject  
/// - InputController must exist in LoginScene as a prefab/GameObject
/// 
/// If managers are missing, clear warnings will be logged.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Manager References (Scene-Based)")]
    [Tooltip("Assigned via scene prefabs or found at runtime - never created dynamically")]
    public AuthManager authManager;
    public NetworkManager networkManager;
    public InputController inputController;

    private void Awake()
    {
        // Singleton pattern - no dynamic creation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Find existing managers in scene - DO NOT CREATE THEM
        // Using FindObjectOfType instead of new GameObject() to respect Unity lifecycle
        FindManagers();
        ValidateManagers();
    }

    /// <summary>
    /// Finds existing managers in the scene without creating new ones.
    /// Replaced dynamic GameObject creation to avoid Unity lifecycle violations.
    /// </summary>
    private void FindManagers()
    {
        // Only find if not already assigned in inspector
        if (authManager == null)
        {
            authManager = FindObjectOfType<AuthManager>();
        }

        if (networkManager == null)
        {
            networkManager = FindObjectOfType<NetworkManager>();
        }

        if (inputController == null)
        {
            inputController = FindObjectOfType<InputController>();
        }
    }

    /// <summary>
    /// Validates that all required managers exist.
    /// Provides clear logging for missing components instead of silently failing.
    /// </summary>
    private void ValidateManagers()
    {
        if (authManager == null)
        {
            Debug.LogWarning("[GameManager] AuthManager not found! Please add AuthManager to LoginScene prefab.");
        }
        else
        {
            Debug.Log("[GameManager] AuthManager found and referenced successfully.");
        }

        if (networkManager == null)
        {
            Debug.LogWarning("[GameManager] NetworkManager not found! Please add NetworkManager to LoginScene prefab.");
        }
        else
        {
            Debug.Log("[GameManager] NetworkManager found and referenced successfully.");
        }

        if (inputController == null)
        {
            Debug.LogWarning("[GameManager] InputController not found! Please add InputController to LoginScene prefab.");
        }
        else
        {
            Debug.Log("[GameManager] InputController found and referenced successfully.");
        }
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        // Reserved for future scene orchestration.
        Debug.Log($"[GameManager] Scene changed from {oldScene.name} to {newScene.name}");
    }

    // Public accessors for managers
    public AuthManager GetAuthManager() => authManager;
    public NetworkManager GetNetworkManager() => networkManager;
    public InputController GetInputController() => inputController;
}
