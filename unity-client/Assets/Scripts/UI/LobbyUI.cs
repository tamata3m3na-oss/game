using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PvpGame.Auth;
using PvpGame.Network;
using PvpGame.Utils;

namespace PvpGame.UI
{
    public class LobbyUI : MonoBehaviour
    {
        [Header("Player Info")]
        public TextMeshProUGUI usernameText;
        public TextMeshProUGUI ratingText;
        public TextMeshProUGUI statsText;

        [Header("Queue UI")]
        public GameObject queuePanel;
        public TextMeshProUGUI queueStatusText;
        public TextMeshProUGUI queuePositionText;
        public Button joinQueueButton;
        public Button leaveQueueButton;

        [Header("Match Found UI")]
        public GameObject matchFoundPanel;
        public TextMeshProUGUI opponentNameText;
        public TextMeshProUGUI opponentRatingText;

        [Header("Other")]
        public Button logoutButton;
        public GameObject loadingPanel;

        private AuthManager authManager;
        private NetworkManager networkManager;
        private bool isInQueue = false;
        private int currentMatchId;

        private async void Start()
        {
            authManager = AuthManager.Instance;
            networkManager = NetworkManager.Instance;

            SetupUI();
            SetupEventListeners();

            SetLoading(true);

            bool connected = await networkManager.ConnectAsync();

            SetLoading(false);

            if (!connected)
            {
                AppLogger.LogError("Failed to connect to game server");
                return;
            }

            UpdatePlayerInfo();
        }

        private void SetupUI()
        {
            if (queuePanel != null)
            {
                queuePanel.SetActive(false);
            }

            if (matchFoundPanel != null)
            {
                matchFoundPanel.SetActive(false);
            }

            if (joinQueueButton != null)
            {
                joinQueueButton.gameObject.SetActive(true);
            }

            if (leaveQueueButton != null)
            {
                leaveQueueButton.gameObject.SetActive(false);
            }
        }

        private void SetupEventListeners()
        {
            if (joinQueueButton != null)
            {
                joinQueueButton.onClick.AddListener(OnJoinQueue);
            }

            if (leaveQueueButton != null)
            {
                leaveQueueButton.onClick.AddListener(OnLeaveQueue);
            }

            if (logoutButton != null)
            {
                logoutButton.onClick.AddListener(OnLogout);
            }

            networkManager.OnQueueStatus += HandleQueueStatus;
            networkManager.OnMatchFound += HandleMatchFound;
            networkManager.OnMatchStart += HandleMatchStart;
        }

        private void OnDestroy()
        {
            if (networkManager != null)
            {
                networkManager.OnQueueStatus -= HandleQueueStatus;
                networkManager.OnMatchFound -= HandleMatchFound;
                networkManager.OnMatchStart -= HandleMatchStart;
            }
        }

        private void UpdatePlayerInfo()
        {
            var user = authManager.CurrentUser;
            if (user == null) return;

            if (usernameText != null)
            {
                usernameText.text = user.username;
            }

            if (ratingText != null)
            {
                ratingText.text = $"Rating: {user.rating}";
            }

            if (statsText != null)
            {
                statsText.text = $"Wins: {user.wins} | Losses: {user.losses}";
            }
        }

        private async void OnJoinQueue()
        {
            AppLogger.Log("Joining matchmaking queue");
            await networkManager.SendEventAsync("queue:join");

            isInQueue = true;

            if (queuePanel != null)
            {
                queuePanel.SetActive(true);
            }

            if (joinQueueButton != null)
            {
                joinQueueButton.gameObject.SetActive(false);
            }

            if (leaveQueueButton != null)
            {
                leaveQueueButton.gameObject.SetActive(true);
            }
        }

        private async void OnLeaveQueue()
        {
            AppLogger.Log("Leaving matchmaking queue");
            await networkManager.SendEventAsync("queue:leave");

            isInQueue = false;

            if (queuePanel != null)
            {
                queuePanel.SetActive(false);
            }

            if (joinQueueButton != null)
            {
                joinQueueButton.gameObject.SetActive(true);
            }

            if (leaveQueueButton != null)
            {
                leaveQueueButton.gameObject.SetActive(false);
            }
        }

        private void HandleQueueStatus(QueueStatusData data)
        {
            if (!isInQueue) return;

            if (queuePositionText != null)
            {
                queuePositionText.text = $"Position: {data.position}";
            }

            if (queueStatusText != null)
            {
                queueStatusText.text = $"Estimated wait: {data.estimatedWait}s";
            }
        }

        private async void HandleMatchFound(MatchFoundData data)
        {
            AppLogger.LogSuccess($"Match found! Opponent: {data.opponent.username}");

            currentMatchId = data.matchId;
            isInQueue = false;

            if (queuePanel != null)
            {
                queuePanel.SetActive(false);
            }

            if (matchFoundPanel != null)
            {
                matchFoundPanel.SetActive(true);
            }

            if (opponentNameText != null)
            {
                opponentNameText.text = data.opponent.username;
            }

            if (opponentRatingText != null)
            {
                opponentRatingText.text = $"Rating: {data.opponent.rating}";
            }

            await networkManager.SendEventAsync("match:ready", new MatchReadyData { matchId = data.matchId });
        }

        private void HandleMatchStart(MatchStartData data)
        {
            AppLogger.LogSuccess($"Match starting! You are {data.color}");

            PlayerPrefs.SetInt("CurrentMatchId", data.matchId);
            PlayerPrefs.SetInt("LocalPlayerId", authManager.CurrentUser.id);
            PlayerPrefs.SetInt("OpponentId", data.opponent.id);
            PlayerPrefs.Save();

            StartCoroutine(LoadGameScene());
        }

        private IEnumerator LoadGameScene()
        {
            yield return new WaitForSeconds(1f);
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }

        private async void OnLogout()
        {
            authManager.Logout();
            await networkManager.DisconnectAsync();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
        }

        private void SetLoading(bool loading)
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(loading);
            }
        }
    }
}
