using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using ShipBattle.Network;
using ShipBattle.Core;

public class LobbyController : MonoBehaviour
{
    [SerializeField] private Text usernameText;
    [SerializeField] private Text ratingText;
    [SerializeField] private Button joinQueueButton;
    [SerializeField] private Button leaveQueueButton;
    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private Text statusText;
    
    private bool inQueue = false;
    
    private void Awake()
    {
        Debug.Log("[LOBBY] LobbyController Awake");
    }
    
    private void Start()
    {
        Debug.Log("[LOBBY] LobbyController Start");
        
        // Subscribe to SocketClient events
        if (SocketClient.Instance != null)
        {
            SocketClient.Instance.OnMatchReady += OnMatchReady;
            SocketClient.Instance.OnError += OnSocketError;
            Debug.Log("[LOBBY] Subscribed to SocketClient events");
        }
        else
        {
            Debug.LogError("[LOBBY] SocketClient instance is null!");
        }
        
        if (joinQueueButton != null)
        {
            joinQueueButton.onClick.AddListener(OnJoinQueue);
            Debug.Log("[LOBBY] Join queue button listener added");
        }
        else
        {
            Debug.LogError("[LOBBY] Join queue button is not assigned!");
        }
        
        if (leaveQueueButton != null)
        {
            leaveQueueButton.onClick.AddListener(OnLeaveQueue);
            leaveQueueButton.gameObject.SetActive(false);
            Debug.Log("[LOBBY] Leave queue button listener added");
        }
        else
        {
            Debug.LogError("[LOBBY] Leave queue button is not assigned!");
        }
        
        if (leaveLobbyButton != null)
        {
            leaveLobbyButton.onClick.AddListener(OnLeaveLobby);
        }
        
        // Load player info
        Debug.Log("[LOBBY] Loading player info...");
        _ = LoadPlayerInfo();
    }
    
    private void OnDestroy()
    {
        Debug.Log("[LOBBY] LobbyController OnDestroy - Unsubscribing from events");
        if (SocketClient.Instance != null)
        {
            SocketClient.Instance.OnMatchReady -= OnMatchReady;
            SocketClient.Instance.OnError -= OnSocketError;
        }
        
        if (joinQueueButton != null)
            joinQueueButton.onClick.RemoveListener(OnJoinQueue);
        if (leaveQueueButton != null)
            leaveQueueButton.onClick.RemoveListener(OnLeaveQueue);
    }
    
    private async Task LoadPlayerInfo()
    {
        try
        {
            Debug.Log("[LOBBY] Fetching player profile...");
            var player = await AuthService.Instance.GetPlayerProfileAsync();
            if (player != null)
            {
                Debug.Log($"[LOBBY] Player profile loaded: {player.username}, Rating: {player.rating}");
                if (usernameText != null) 
                {
                    usernameText.text = $"Welcome, {player.username}";
                    Debug.Log("[LOBBY] Username text updated");
                }
                if (ratingText != null) 
                {
                    ratingText.text = $"Rating: {player.rating}";
                    Debug.Log("[LOBBY] Rating text updated");
                }
            }
            else
            {
                Debug.LogWarning("[LOBBY] Player profile is null");
                if (statusText != null) statusText.text = "Error loading profile";
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LOBBY] Error loading player: {ex.Message}");
            Debug.LogError($"[LOBBY] Stack trace: {ex.StackTrace}");
            if (statusText != null) statusText.text = "Error loading profile";
        }
    }
    
    private void OnJoinQueue()
    {
        Debug.Log("[LOBBY] Join queue button clicked");
        
        if (inQueue) 
        {
            Debug.Log("[LOBBY] Already in queue, ignoring");
            return;
        }
        
        inQueue = true;
        if (joinQueueButton != null) joinQueueButton.gameObject.SetActive(false);
        if (leaveQueueButton != null) leaveQueueButton.gameObject.SetActive(true);
        if (statusText != null) 
        {
            statusText.text = "Searching for opponent...";
            statusText.color = Color.yellow;
        }
        
        Debug.Log("[LOBBY] Sending queue:join event");
        SocketClient.Instance.SendEvent("queue:join", new { });
        Debug.Log("[LOBBY] Joined queue");
    }
    
    private void OnLeaveQueue()
    {
        Debug.Log("[LOBBY] Leave queue button clicked");
        
        if (!inQueue) 
        {
            Debug.Log("[LOBBY] Not in queue, ignoring");
            return;
        }
        
        inQueue = false;
        if (joinQueueButton != null) joinQueueButton.gameObject.SetActive(true);
        if (leaveQueueButton != null) leaveQueueButton.gameObject.SetActive(false);
        if (statusText != null) 
        {
            statusText.text = "Queue cancelled";
            statusText.color = Color.white;
        }
        
        Debug.Log("[LOBBY] Sending queue:leave event");
        SocketClient.Instance.SendEvent("queue:leave", new { });
        Debug.Log("[LOBBY] Left queue");
    }

    private void OnLeaveLobby()
    {
        Debug.Log("[LOBBY] Leave Lobby clicked");
        if (SocketClient.Instance != null && SocketClient.Instance.IsConnected)
        {
            _ = SocketClient.Instance.DisconnectAsync();
        }
        SceneManager.LoadScene("Login");
    }
    
    private void OnMatchReady(MatchReadyEvent matchData)
    {
        Debug.Log($"[LOBBY] Match ready event received! Opponent: {matchData.opponentUsername}");
        
        // Store match data for Game scene
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentMatch(matchData);
            Debug.Log("[LOBBY] Match data stored in GameManager");
        }
        else
        {
            Debug.LogError("[LOBBY] GameManager instance is null!");
        }
        
        Debug.Log("[LOBBY] Loading Game scene...");
        SceneManager.LoadScene("Game");
    }
    
    private void OnSocketError(string error)
    {
        Debug.LogError($"[LOBBY] Socket error: {error}");
        if (statusText != null) 
        {
            statusText.text = $"Error: {error}";
            statusText.color = Color.red;
        }
    }
}
