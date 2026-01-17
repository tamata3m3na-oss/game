using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Managers")]
    public AuthManager authManager;
    public NetworkManager networkManager;
    public InputController inputController;
    public GameStateManager gameStateManager;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Initialize managers if not already present
        InitializeManagers();
    }
    
    private void InitializeManagers()
    {
        // Check if auth manager exists
        if (authManager == null)
        {
            authManager = FindObjectOfType<AuthManager>();
            if (authManager == null)
            {
                GameObject authObj = new GameObject("AuthManager");
                authManager = authObj.AddComponent<AuthManager>();
                DontDestroyOnLoad(authObj);
            }
        }
        
        // Check if network manager exists
        if (networkManager == null)
        {
            networkManager = FindObjectOfType<NetworkManager>();
            if (networkManager == null)
            {
                GameObject networkObj = new GameObject("NetworkManager");
                networkManager = networkObj.AddComponent<NetworkManager>();
                DontDestroyOnLoad(networkObj);
            }
        }
        
        // Check if input controller exists
        if (inputController == null)
        {
            inputController = FindObjectOfType<InputController>();
            if (inputController == null)
            {
                GameObject inputObj = new GameObject("InputController");
                inputController = inputObj.AddComponent<InputController>();
                DontDestroyOnLoad(inputObj);
            }
        }
        
        // Game state manager is scene-specific, don't create here
    }
    
    private void Start()
    {
        // Set up scene-specific initialization
        SceneManager.activeSceneChanged += OnSceneChanged;
        
        // Initialize current scene
        InitializeCurrentScene();
    }
    
    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        InitializeCurrentScene();
    }
    
    private void InitializeCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        switch (sceneName)
        {
            case "LoginScene":
                InitializeLoginScene();
                break;
                
            case "LobbyScene":
                InitializeLobbyScene();
                break;
                
            case "GameScene":
                InitializeGameScene();
                break;
                
            case "ResultScene":
                InitializeResultScene();
                break;
        }
    }
    
    private void InitializeLoginScene()
    {
        Debug.Log("Initializing Login Scene");
        
        // Make sure we have a login UI controller
        LoginUIController loginUI = FindObjectOfType<LoginUIController>();
        if (loginUI == null)
        {
            Debug.LogError("No LoginUIController found in LoginScene");
        }
    }
    
    private void InitializeLobbyScene()
    {
        Debug.Log("Initializing Lobby Scene");
        
        // Make sure we have a lobby UI controller
        LobbyUIController lobbyUI = FindObjectOfType<LobbyUIController>();
        if (lobbyUI == null)
        {
            Debug.LogError("No LobbyUIController found in LobbyScene");
        }
        
        // Connect to network if not already connected
        if (authManager.IsLoggedIn() && !networkManager.IsConnected())
        {
            networkManager.Initialize(authManager.GetAuthToken());
        }
    }
    
    private void InitializeGameScene()
    {
        Debug.Log("Initializing Game Scene");
        
        // Make sure we have a game state manager
        gameStateManager = FindObjectOfType<GameStateManager>();
        if (gameStateManager == null)
        {
            Debug.LogError("No GameStateManager found in GameScene");
        }
        
        // Set up input controller to send inputs to network
        if (inputController != null)
        {
            inputController.OnInputEvent += HandleInputEvent;
        }
        
        // Subscribe to match start event
        networkManager.OnMatchStart.AddListener(HandleMatchStart);
    }
    
    private void InitializeResultScene()
    {
        Debug.Log("Initializing Result Scene");
        
        // Make sure we have a result screen controller
        ResultScreenController resultUI = FindObjectOfType<ResultScreenController>();
        if (resultUI == null)
        {
            Debug.LogError("No ResultScreenController found in ResultScene");
        }
    }
    
    private void HandleInputEvent(GameInputData input)
    {
        if (networkManager != null && networkManager.IsConnected())
        {
            networkManager.SendGameInput(input);
        }
    }
    
    private void HandleMatchStart(NetworkManager.MatchStartData data)
    {
        if (gameStateManager != null)
        {
            int localPlayerId = authManager.GetUserId();
            int opponentPlayerId = data.opponent.id;
            
            gameStateManager.StartGame(data.matchId, localPlayerId, opponentPlayerId);
            
            // Update UI with opponent name
            MatchUIController matchUI = FindObjectOfType<MatchUIController>();
            if (matchUI != null)
            {
                matchUI.SetOpponentName(data.opponent.username);
            }
        }
    }
    
    private void OnDestroy()
    {
        if (inputController != null)
        {
            inputController.OnInputEvent -= HandleInputEvent;
        }
        
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }
}