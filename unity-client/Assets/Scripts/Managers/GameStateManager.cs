using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Scene-local runtime view/controller for the current match.
/// Subscribes to network events on enable and updates scene objects (ships/UI).
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

    private void OnEnable()
    {
        var nem = NetworkEventManager.Instance;
        if (nem != null)
        {
            nem.OnMatchStartReceived += HandleMatchStart;
            nem.OnGameSnapshotReceived += HandleGameSnapshot;
            nem.OnGameEndReceived += HandleGameEnd;
        }

        if (InputController.Instance != null)
        {
            InputController.Instance.OnInputEvent += HandleInputEvent;
        }
    }

    private void OnDisable()
    {
        var nem = NetworkEventManager.Instance;
        if (nem != null)
        {
            nem.OnMatchStartReceived -= HandleMatchStart;
            nem.OnGameSnapshotReceived -= HandleGameSnapshot;
            nem.OnGameEndReceived -= HandleGameEnd;
        }

        if (InputController.Instance != null)
        {
            InputController.Instance.OnInputEvent -= HandleInputEvent;
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
        var network = NetworkManager.Instance;
        if (network != null && network.IsConnected())
        {
            network.SendGameInput(input);
        }
    }

    private void HandleMatchStart(MatchStartData data)
    {
        if (data == null) return;

        currentMatchId = data.matchId;

        localPlayerId = AuthManager.Instance != null ? AuthManager.Instance.GetUserId() : -1;
        opponentPlayerId = data.opponent != null ? data.opponent.id : -1;

        InitializeShips();

        Debug.Log($"[GameStateManager] Match start: {currentMatchId} local={localPlayerId} opponent={opponentPlayerId}");
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
        if (playerShip == null || opponentShip == null) return;
        if (state.player1 == null || state.player2 == null) return;

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
