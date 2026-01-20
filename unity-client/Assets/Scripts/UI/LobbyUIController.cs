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

    private bool isInQueue;

    private void Start()
    {
        UpdatePlayerStats();

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

        UpdateQueueButtonState();

        var nem = NetworkEventManager.GetInstance(false);
        if (nem != null)
        {
            nem.OnQueueStatusReceived += HandleQueueStatus;
            nem.OnMatchFoundReceived += HandleMatchFound;
        }

        if (queuePanel != null)
        {
            queuePanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        var nem = NetworkEventManager.GetInstance(false);
        if (nem != null)
        {
            nem.OnQueueStatusReceived -= HandleQueueStatus;
            nem.OnMatchFoundReceived -= HandleMatchFound;
        }
    }

    private void UpdatePlayerStats()
    {
        if (usernameText != null && AuthManager.Instance != null)
        {
            usernameText.text = AuthManager.Instance.GetUsername();
        }

        if (ratingText != null) ratingText.text = "1000";
        if (winsText != null) winsText.text = "10";
        if (lossesText != null) lossesText.text = "5";
    }

    private void OnQueueButtonClicked()
    {
        if (isInQueue) return;

        NetworkManager.Instance?.JoinQueue();
        isInQueue = true;
        UpdateQueueButtonState();

        if (queuePanel != null)
        {
            queuePanel.SetActive(true);
        }
    }

    private void OnLeaveQueueButtonClicked()
    {
        if (!isInQueue) return;

        NetworkManager.Instance?.LeaveQueue();
        isInQueue = false;
        UpdateQueueButtonState();

        if (queuePanel != null)
        {
            queuePanel.SetActive(false);
        }
    }

    private void OnDisconnectButtonClicked()
    {
        AuthManager.Instance?.Logout();
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

    private void HandleQueueStatus(QueueStatusData status)
    {
        if (queueStatusText == null || status == null) return;

        if (status.position > 0)
        {
            queueStatusText.text = "Position: " + status.position + " | Estimated Wait: " + status.estimatedWait + "s";
        }
        else
        {
            queueStatusText.text = "Searching for match...";
        }
    }

    private void HandleMatchFound(MatchFoundData data)
    {
        if (data == null) return;

        Debug.Log("Match found! Match ID: " + data.matchId + " Opponent: " + data.opponent?.username);

        NetworkManager.Instance?.MarkMatchReady(data.matchId);
        SceneManager.LoadScene("GameScene");
    }

    private void LoadLeaderboard()
    {
        if (leaderboardContent == null || leaderboardEntryPrefab == null) return;

        foreach (Transform child in leaderboardContent)
        {
            Destroy(child.gameObject);
        }

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
