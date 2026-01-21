using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PvpGame.Game;
using PvpGame.Auth;
using PvpGame.Utils;

namespace PvpGame.UI
{
    public class GameUI : MonoBehaviour
    {
        [Header("Player Info")]
        public TextMeshProUGUI playerNameText;
        public TextMeshProUGUI opponentNameText;

        [Header("Game Info")]
        public TextMeshProUGUI matchIdText;
        public TextMeshProUGUI timerText;

        [Header("Status")]
        public TextMeshProUGUI statusText;

        [Header("References")]
        public GameManager gameManager;

        private float matchStartTime;
        private int matchId;
        private int localPlayerId;
        private int opponentId;

        private void Start()
        {
            matchId = PlayerPrefs.GetInt("CurrentMatchId", 0);
            localPlayerId = PlayerPrefs.GetInt("LocalPlayerId", 0);
            opponentId = PlayerPrefs.GetInt("OpponentId", 0);

            if (matchIdText != null)
            {
                matchIdText.text = $"Match #{matchId}";
            }

            if (playerNameText != null)
            {
                playerNameText.text = AuthManager.Instance.CurrentUser?.username ?? "Player";
            }

            if (opponentNameText != null)
            {
                opponentNameText.text = "Opponent";
            }

            matchStartTime = Time.time;

            if (gameManager != null)
            {
                gameManager.StartMatch(matchId, localPlayerId, opponentId);
            }

            ShowStatus("Match Started!");
            Invoke(nameof(HideStatus), 2f);
        }

        private void Update()
        {
            UpdateTimer();
        }

        private void UpdateTimer()
        {
            if (timerText != null)
            {
                float elapsed = Time.time - matchStartTime;
                int minutes = Mathf.FloorToInt(elapsed / 60f);
                int seconds = Mathf.FloorToInt(elapsed % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }
        }

        public void ShowStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.gameObject.SetActive(true);
            }
        }

        public void HideStatus()
        {
            if (statusText != null)
            {
                statusText.gameObject.SetActive(false);
            }
        }
    }
}
