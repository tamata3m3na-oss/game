using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using ShipBattle.Network;
using ShipBattle.Core;
using TMPro;

public class LobbyController : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text playerRankText;
    [SerializeField] private Button queueButton;
    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private TMP_Text queueStatusText;
    
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
        }
        
        if (queueButton != null)
        {
            queueButton.onClick.AddListener(QueueForMatch);
        }
        
        if (leaveLobbyButton != null)
        {
            leaveLobbyButton.onClick.AddListener(LeaveLobby);
        }
        
        // Load player info
        _ = LoadPlayerInfo();
    }
    
    private void OnDestroy()
    {
        Debug.Log("[LOBBY] LobbyController OnDestroy");
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
                if (playerNameText != null) playerNameText.text = player.username;
                if (playerRankText != null) playerRankText.text = $"Rank: {player.rating}";
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LOBBY] Error loading player: {ex.Message}");
        }
    }
    
    public void QueueForMatch()
    {
        Debug.Log("[LOBBY] QueueForMatch called");
        
        if (inQueue) 
        {
            // If already in queue, clicking again cancels it
            CancelQueue();
            return;
        }
        
        inQueue = true;
        if (queueStatusText != null) 
        {
            queueStatusText.text = "Searching for match...";
            queueStatusText.color = Color.yellow;
        }
        
        if (queueButton != null)
        {
            var text = queueButton.GetComponentInChildren<TMP_Text>();
            if (text != null) text.text = "Cancel Queue";
        }
        
        SocketClient.Instance.SendEvent("queue:join", new { });
        Debug.Log("[LOBBY] Joined queue");
    }
    
    private void CancelQueue()
    {
        inQueue = false;
        if (queueStatusText != null) 
        {
            queueStatusText.text = "Queue cancelled";
            queueStatusText.color = Color.white;
        }
        
        if (queueButton != null)
        {
            var text = queueButton.GetComponentInChildren<TMP_Text>();
            if (text != null) text.text = "Find Match";
        }
        
        SocketClient.Instance.SendEvent("queue:leave", new { });
        Debug.Log("[LOBBY] Left queue");
    }

    public void LeaveLobby()
    {
        Debug.Log("[LOBBY] LeaveLobby called");
        if (SocketClient.Instance != null && SocketClient.Instance.IsConnected)
        {
            _ = SocketClient.Instance.DisconnectAsync();
        }
        SceneManager.LoadScene("Login");
    }
    
    private void OnMatchReady(MatchReadyEvent matchData)
    {
        Debug.Log($"[LOBBY] Match ready! Opponent: {matchData.opponentUsername}");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentMatch(matchData);
        }
        
        SceneManager.LoadScene("Game");
    }
    
    private void OnSocketError(string error)
    {
        Debug.LogError($"[LOBBY] Socket error: {error}");
        if (queueStatusText != null) 
        {
            queueStatusText.text = $"Error: {error}";
            queueStatusText.color = Color.red;
        }
    }
}
