using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Scene-local runtime view/controller for the current match.
/// Subscribes to network events on enable and updates scene objects (ships/UI).
/// This manager is scene-specific and should not persist across scene loads.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    [Header("References")]
    public ShipController playerShip;
    public ShipController opponentShip;
    public GameObject playerShipPrefab;
    public GameObject opponentShipPrefab;
    public Transform playerSpawnPoint;
    public Transform opponentSpawnPoint;
    
    [Header("GameObjectSpawner")]
    public GameObjectSpawner objectSpawner;

    [Header("Game Settings")]
    public int localPlayerId = -1;
    public int opponentPlayerId = -1;
    public int currentMatchId = -1;

    private NetworkGameState currentGameState;
    private int lastProcessedTick = -1;
    private float snapshotDelay;

    private int fpsCounter;
    private float fpsTimer;
    private int currentFPS;

    public UnityEvent<int> OnGameEnded = new UnityEvent<int>();

    private void Awake()
    {
        // Validate that GameStateManager is in the scene
        if (GetComponent<GameStateManager>() == null)
        {
            Debug.LogError("GameStateManager component validation failed");
        }
        
        // Find GameObjectSpawner if not assigned
        if (objectSpawner == null)
        {
            objectSpawner = FindObjectOfType<GameObjectSpawner>();
        }
    }

    private void OnEnable()
    {
        // Find NetworkEventManager from scene (scene-local pattern)
        var nem = FindObjectOfType<NetworkEventManager>();
        if (nem != null)
        {
            nem.OnMatchStartReceived += HandleMatchStart;
            nem.OnGameSnapshotReceived += HandleGameSnapshot;
            nem.OnGameEndReceived += HandleGameEnd;
        }
        else
        {
            Debug.LogWarning("[GameStateManager] NetworkEventManager not found in scene");
        }

        // Find InputController from scene (scene-local pattern)
        var inputController = FindObjectOfType<InputController>();
        if (inputController != null)
        {
            inputController.OnInputEvent += HandleInputEvent;
        }
        else
        {
            Debug.LogWarning("[GameStateManager] InputController not found in scene");
        }
    }

    private void OnDisable()
    {
        // Find NetworkEventManager from scene (scene-local pattern)
        var nem = FindObjectOfType<NetworkEventManager>();
        if (nem != null)
        {
            nem.OnMatchStartReceived -= HandleMatchStart;
            nem.OnGameSnapshotReceived -= HandleGameSnapshot;
            nem.OnGameEndReceived -= HandleGameEnd;
        }

        // Find InputController from scene (scene-local pattern)
        var inputController = FindObjectOfType<InputController>();
        if (inputController != null)
        {
            inputController.OnInputEvent -= HandleInputEvent;
        }
    }

    private void Update()
    {
        fpsCounter++;
        fpsTimer += Time.deltaTime;

        if (fpsTimer >= 1.0f)
        {
            currentFPS = fpsCounter;
            fpsCounter = 0;
            fpsTimer = 0f;
        }
    }

    private void HandleInputEvent(GameInputData input)
    {
        // Find NetworkManager from scene (scene-local pattern)
        var network = FindObjectOfType<NetworkManager>();
        if (network != null && network.IsConnected())
        {
            network.SendGameInput(input);
        }
        else
        {
            Debug.LogWarning("[GameStateManager] NetworkManager not found or not connected");
        }
    }

    private void HandleMatchStart(MatchStartData data)
    {
        if (data == null) return;

        currentMatchId = data.matchId;

        // Find AuthManager from scene (scene-local pattern)
        var authManager = FindObjectOfType<AuthManager>();
        localPlayerId = authManager != null ? authManager.GetUserId() : -1;
        opponentPlayerId = data.opponent != null ? data.opponent.id : -1;

        InitializeShips();

        Debug.Log($"[GameStateManager] Match start: {currentMatchId} local={localPlayerId} opponent={opponentPlayerId}");
    }

    /// <summary>
    /// إنشاء الطائرات (Public method يمكن استدعاؤها من أي مكان)
    /// </summary>
    public void InitializeShipsFromSnapshot(NetworkGameState snapshotData)
    {
        if (snapshotData == null || snapshotData.player1 == null || snapshotData.player2 == null)
        {
            Debug.LogWarning("[GameStateManager] Cannot initialize ships - invalid snapshot data");
            return;
        }

        try
        {
            // تحديد IDs اللاعبين من البيانات
            if (snapshotData.player1.id > 0 && snapshotData.player2.id > 0)
            {
                // استخدام IDs من البيانات
                localPlayerId = snapshotData.player1.id; // سيتم تصحيح هذا لاحقاً عند وصول بيانات Auth
                opponentPlayerId = snapshotData.player2.id;
                
                // محاولة تحديد اللاعب المحلي من AuthManager
                var authManager = FindObjectOfType<AuthManager>();
                if (authManager != null)
                {
                    int authUserId = authManager.GetUserId();
                    if (authUserId > 0)
                    {
                        localPlayerId = authUserId;
                        opponentPlayerId = (snapshotData.player1.id == authUserId) ? snapshotData.player2.id : snapshotData.player1.id;
                    }
                }
            }

            InitializeShips();
            Debug.Log($"[GameStateManager] Ships initialized from snapshot: local={localPlayerId} opponent={opponentPlayerId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameStateManager] Error initializing ships from snapshot: {e.Message}");
        }
    }

    private void InitializeShips()
    {
        if (playerShip != null)
        {
            Destroy(playerShip.gameObject);
            playerShip = null;
        }

        if (opponentShip != null)
        {
            Destroy(opponentShip.gameObject);
            opponentShip = null;
        }

        if (playerShipPrefab != null && playerSpawnPoint != null)
        {
            GameObject playerObj = Instantiate(playerShipPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
            playerShip = playerObj.GetComponent<ShipController>();
            playerShip?.Initialize(localPlayerId, true);
        }

        if (opponentShipPrefab != null && opponentSpawnPoint != null)
        {
            GameObject opponentObj = Instantiate(opponentShipPrefab, opponentSpawnPoint.position, opponentSpawnPoint.rotation);
            opponentShip = opponentObj.GetComponent<ShipController>();
            opponentShip?.Initialize(opponentPlayerId, false);
        }
    }

    private void HandleGameSnapshot(NetworkGameState state)
    {
        if (state == null) return;

        float currentTime = Time.time;
        float serverTime = state.timestamp / 1000f;
        snapshotDelay = currentTime - serverTime;

        currentGameState = state;
        lastProcessedTick = state.tick;

        UpdateShipsFromState(state);
    }

    private void UpdateShipsFromState(NetworkGameState state)
    {
        if (state == null || state.player1 == null || state.player2 == null) return;

        // التأكد من إنشاء الطائرات أولاً
        EnsureShipsExist();

        if (playerShip == null || opponentShip == null) return;

        if (state.player1.id == localPlayerId)
        {
            playerShip.UpdateFromSnapshot(state.player1);
            opponentShip.UpdateFromSnapshot(state.player2);
        }
        else
        {
            playerShip.UpdateFromSnapshot(state.player2);
            opponentShip.UpdateFromSnapshot(state.player1);
        }
        
        // تحديث GameObjectSpawner أيضاً
        if (objectSpawner != null)
        {
            objectSpawner.UpdateShipsFromSnapshot(state);
        }
    }

    /// <summary>
    /// التأكد من وجود الطائرات
    /// </summary>
    private void EnsureShipsExist()
    {
        if (playerShip == null || opponentShip == null)
        {
            if (objectSpawner != null)
            {
                playerShip = objectSpawner.GetPlayerShip();
                opponentShip = objectSpawner.GetOpponentShip();
            }
        }
    }

    private void HandleGameEnd(GameEndData data)
    {
        if (data == null) return;

        Debug.Log($"[GameStateManager] Game ended. Winner: {data.winner}");
        OnGameEnded.Invoke(data.winner);

        if (data.finalState != null)
        {
            currentGameState = data.finalState;
        }
    }

    public NetworkGameState GetCurrentGameState() => currentGameState;

    public int GetCurrentMatchId() => currentMatchId;

    public int GetLocalPlayerId() => localPlayerId;

    public int GetOpponentPlayerId() => opponentPlayerId;

    public int GetCurrentFPS() => currentFPS;

    public float GetSnapshotDelay() => snapshotDelay;

    public ShipController GetPlayerShip() => playerShip;

    public ShipController GetOpponentShip() => opponentShip;
}
