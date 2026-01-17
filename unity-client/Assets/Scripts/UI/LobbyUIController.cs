using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyUIController : MonoBehaviour
{
    [Header("Player Stats")]
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI ratingText;
    public TextMeshProUGUI winsText;
    public TextMeshProUGUI lossesText;
    
    [Header("Queue System")]
    public Button queueButton;
    public Button leaveQueueButton;
    public TextMeshProUGUI queueStatusText;
    public GameObject queuePanel;
    
    [Header("Leaderboard")]
    public Transform leaderboardContent;
    public GameObject leaderboardEntryPrefab;
    
    [Header("Other")]
    public Button disconnectButton;
    
    private bool isInQueue = false;
    
    private void Start()
    {
        // Initialize UI
        UpdatePlayerStats();
        
        // Set up buttons
        if (queueButton != null)
        {
            queueButton.onClick.AddListener(OnQueueButtonClicked);
        }
        
        if (leaveQueueButton != null)
        {
            leaveQueueButton.onClick.AddListener(OnLeaveQueueButtonClicked);
        }
        
        if (disconnectButton != null)
        {
            disconnectButton.onClick.AddListener(OnDisconnectButtonClicked);
        }
        
        // Update queue button state
        UpdateQueueButtonState();
        
        // Subscribe to network events
        NetworkManager.Instance.OnQueueStatus.AddListener(HandleQueueStatus);
        NetworkManager.Instance.OnMatchFound.AddListener(HandleMatchFound);
        
        // Hide queue panel initially
        if (queuePanel != null)
        {
            queuePanel.SetActive(false);
        }
    }
    
    private void OnDestroy()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnQueueStatus.RemoveListener(HandleQueueStatus);
            NetworkManager.Instance.OnMatchFound.RemoveListener(HandleMatchFound);
        }
    }
    
    private void UpdatePlayerStats()
    {
        if (usernameText != null)
        {
            usernameText.text = AuthManager.Instance.GetUsername();
        }
        
        // Rating, wins, losses would come from player stats API
        // This is placeholder data
        if (ratingText != null) ratingText.text = "1000";
        if (winsText != null) winsText.text = "10";
        if (lossesText != null) lossesText.text = "5";
    }
    
    private void OnQueueButtonClicked()
    {
        if (!isInQueue)
        {
            NetworkManager.Instance.JoinQueue();
            isInQueue = true;
            UpdateQueueButtonState();
            
            if (queuePanel != null)
            {
                queuePanel.SetActive(true);
            }
        }
    }
    
    private void OnLeaveQueueButtonClicked()
    {
        if (isInQueue)
        {
            NetworkManager.Instance.LeaveQueue();
            isInQueue = false;
            UpdateQueueButtonState();
            
            if (queuePanel != null)
            {
                queuePanel.SetActive(false);
            }
        }
    }
    
    private void OnDisconnectButtonClicked()
    {
        AuthManager.Instance.Logout();
        SceneManager.LoadScene("LoginScene");
    }
    
    private void UpdateQueueButtonState()
    {
        if (queueButton != null)
        {
            queueButton.gameObject.SetActive(!isInQueue);
        }
        
        if (leaveQueueButton != null)
        {
            leaveQueueButton.gameObject.SetActive(isInQueue);
        }
    }
    
    private void HandleQueueStatus(NetworkManager.QueueStatus status)
    {
        if (queueStatusText != null)
        {
            if (status.position > 0)
            {
                queueStatusText.text = "Position: " + status.position + " | Estimated Wait: " + status.estimatedWait + "s";
            }
            else
            {
                queueStatusText.text = "Searching for match...";
            }
        }
    }
    
    private void HandleMatchFound(NetworkManager.MatchFoundData data)
    {
        Debug.Log("Match found! Match ID: " + data.matchId + " Opponent: " + data.opponent.username);
        
        // Mark player as ready for the match
        NetworkManager.Instance.MarkMatchReady(data.matchId);
        
        // Load game scene
        SceneManager.LoadScene("GameScene");
    }
    
    private void LoadLeaderboard()
    {
        // This would call a leaderboard API and populate the leaderboard
        // For now, just show placeholder data
        
        if (leaderboardContent != null && leaderboardEntryPrefab != null)
        {
            // Clear existing entries
            foreach (Transform child in leaderboardContent)
            {
                Destroy(child.gameObject);
            }
            
            // Add placeholder entries
            for (int i = 0; i < 10; i++)
            {
                GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContent);
                LeaderboardEntry entry = entryObj.GetComponent<LeaderboardEntry>();
                if (entry != null)
                {
                    entry.SetData(i + 1, "Player" + (i + 1), 1000 + (10 - i) * 50, 20 - i, i);
                }
            }
        }
    }
}