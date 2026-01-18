using System;
using UnityEngine;
using UnityEngine.Events;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    
    [Header("References")]
    public ShipController playerShip;
    public ShipController opponentShip;
    public GameObject playerShipPrefab;
    public GameObject opponentShipPrefab;
    public Transform playerSpawnPoint;
    public Transform opponentSpawnPoint;
    
    [Header("Game Settings")]
    public int localPlayerId = -1;
    public int opponentPlayerId = -1;
    public int currentMatchId = -1;
    
    // Game state
    private NetworkManager.NetworkGameState currentGameState;
    private int lastProcessedTick = -1;
    private float snapshotDelay = 0f;
    private int fpsCounter = 0;
    private float fpsTimer = 0f;
    private int currentFPS = 0;
    
    // Events
    public UnityEvent<int> OnGameEnded = new UnityEvent<int>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Subscribe to network events
        NetworkManager.Instance.OnGameSnapshot.AddListener(HandleGameSnapshot);
        NetworkManager.Instance.OnGameEnd.AddListener(HandleGameEnd);
        
        // Initialize ships
        InitializeShips();
    }
    
    private void Update()
    {
        // Calculate FPS
        fpsCounter++;
        fpsTimer += Time.deltaTime;
        
        if (fpsTimer >= 1.0f)
        {
            currentFPS = fpsCounter;
            fpsCounter = 0;
            fpsTimer = 0f;
        }
    }
    
    private void InitializeShips()
    {
        // Create player ship
        if (playerShipPrefab != null && playerSpawnPoint != null)
        {
            GameObject playerObj = Instantiate(playerShipPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
            playerShip = playerObj.GetComponent<ShipController>();
            if (playerShip != null)
            {
                playerShip.Initialize(localPlayerId, true);
            }
        }
        
        // Create opponent ship
        if (opponentShipPrefab != null && opponentSpawnPoint != null)
        {
            GameObject opponentObj = Instantiate(opponentShipPrefab, opponentSpawnPoint.position, opponentSpawnPoint.rotation);
            opponentShip = opponentObj.GetComponent<ShipController>();
            if (opponentShip != null)
            {
                opponentShip.Initialize(opponentPlayerId, false);
            }
        }
    }
    
    public void StartGame(int matchId, int playerId, int opponentId)
    {
        currentMatchId = matchId;
        localPlayerId = playerId;
        opponentPlayerId = opponentId;
        
        Debug.Log("Game started: Match " + matchId + " Player " + playerId + " vs " + opponentId);
    }
    
    private void HandleGameSnapshot(NetworkManager.NetworkGameState state)
    {
        if (state == null) return;

        // Calculate snapshot delay
        float currentTime = Time.time;
        float serverTime = state.timestamp / 1000f; // Convert milliseconds to seconds
        snapshotDelay = currentTime - serverTime;
        
        // Check for lag
        if (snapshotDelay > 0.2f) // 200ms
        {
            Debug.LogWarning("High latency detected: " + snapshotDelay + " seconds");
        }
        
        // Store current state
        currentGameState = state;
        
        // Update ships based on game state
        UpdateShipsFromState(state);
        
        // Check for anti-cheat violations
        CheckForAntiCheatViolations(state);
        
        lastProcessedTick = state.tick;
    }
    
    private void UpdateShipsFromState(NetworkManager.NetworkGameState state)
    {
        if (playerShip != null && opponentShip != null)
        {
            // Determine which player is which
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
        }
    }
    
    private void CheckForAntiCheatViolations(NetworkManager.NetworkGameState state)
    {
        // Check for position jumps
        if (playerShip != null)
        {
            Vector3 currentPos = playerShip.transform.position;
            Vector3 targetPos = new Vector3(state.player1.x, 0f, state.player1.y);
            
            float distance = Vector3.Distance(currentPos, targetPos);
            if (distance > 5f) // 5 units jump threshold
            {
                Debug.LogWarning("Position jump detected: " + distance + " units");
            }
        }
    }
    
    private void HandleGameEnd(NetworkManager.GameEndData data)
    {
        Debug.Log("Game ended. Winner: " + (data.winner == localPlayerId ? "Local Player" : "Opponent"));
        OnGameEnded.Invoke(data.winner);
    }
    
    public NetworkManager.NetworkGameState GetCurrentGameState()
    {
        return currentGameState;
    }
    
    public int GetCurrentMatchId()
    {
        return currentMatchId;
    }
    
    public int GetLocalPlayerId()
    {
        return localPlayerId;
    }
    
    public int GetOpponentPlayerId()
    {
        return opponentPlayerId;
    }
    
    public int GetCurrentFPS()
    {
        return currentFPS;
    }
    
    public float GetSnapshotDelay()
    {
        return snapshotDelay;
    }
    
    public ShipController GetPlayerShip()
    {
        return playerShip;
    }
    
    public ShipController GetOpponentShip()
    {
        return opponentShip;
    }
}