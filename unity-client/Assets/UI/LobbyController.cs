using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using ShipBattle.Network;
using ShipBattle.Network.Models;

public class LobbyController : MonoBehaviour
{
    [SerializeField] private Text usernameText;
    [SerializeField] private Text ratingText;
    [SerializeField] private Button joinQueueButton;
    [SerializeField] private Button leaveQueueButton;
    [SerializeField] private Text statusText;
    
    private bool inQueue = false;
    
    private void Start()
    {
        // Subscribe to SocketClient events
        SocketClient.Instance.OnMatchReady += OnMatchReady;
        SocketClient.Instance.OnError += OnSocketError;
        
        joinQueueButton.onClick.AddListener(OnJoinQueue);
        leaveQueueButton.onClick.AddListener(OnLeaveQueue);
        
        if (leaveQueueButton != null) leaveQueueButton.gameObject.SetActive(false);
        
        // Load player info
        _ = LoadPlayerInfo();
    }
    
    private void OnDestroy()
    {
        if (SocketClient.Instance != null)
        {
            SocketClient.Instance.OnMatchReady -= OnMatchReady;
            SocketClient.Instance.OnError -= OnSocketError;
        }
    }
    
    private async Task LoadPlayerInfo()
    {
        try
        {
            var player = await AuthService.Instance.GetPlayerProfileAsync();
            if (player != null)
            {
                if (usernameText != null) usernameText.text = $"Welcome, {player.username}";
                if (ratingText != null) ratingText.text = $"Rating: {player.rating}";
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LOBBY] Error loading player: {ex}");
            if (statusText != null) statusText.text = "Error loading profile";
        }
    }
    
    private void OnJoinQueue()
    {
        if (inQueue) return;
        
        inQueue = true;
        if (joinQueueButton != null) joinQueueButton.gameObject.SetActive(false);
        if (leaveQueueButton != null) leaveQueueButton.gameObject.SetActive(true);
        if (statusText != null) 
        {
            statusText.text = "Searching for opponent...";
            statusText.color = Color.yellow;
        }
        
        SocketClient.Instance.SendEvent("queue:join", new { });
        Debug.Log("[LOBBY] Joined queue");
    }
    
    private void OnLeaveQueue()
    {
        if (!inQueue) return;
        
        inQueue = false;
        if (joinQueueButton != null) joinQueueButton.gameObject.SetActive(true);
        if (leaveQueueButton != null) leaveQueueButton.gameObject.SetActive(false);
        if (statusText != null) 
        {
            statusText.text = "Queue cancelled";
            statusText.color = Color.white;
        }
        
        SocketClient.Instance.SendEvent("queue:leave", new { });
        Debug.Log("[LOBBY] Left queue");
    }
    
    private void OnMatchReady(MatchReadyEvent matchData)
    {
        Debug.Log($"[LOBBY] Match ready! Loading Game scene...");
        
        // Store match data for Game scene
        GameManager.Instance.SetCurrentMatch(matchData);
        
        SceneManager.LoadScene("Game");
    }
    
    private void OnSocketError(string error)
    {
        if (statusText != null) 
        {
            statusText.text = $"Error: {error}";
            statusText.color = Color.red;
        }
    }
}
