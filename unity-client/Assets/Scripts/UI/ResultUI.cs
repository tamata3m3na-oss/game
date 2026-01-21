using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PvpGame.Auth;
using PvpGame.Utils;

namespace PvpGame.UI
{
    public class ResultUI : MonoBehaviour
    {
        [Header("Result Display")]
        public TextMeshProUGUI resultText;
        public TextMeshProUGUI eloChangeText;
        public Image resultBackground;

        [Header("Colors")]
        public Color winColor = Color.green;
        public Color loseColor = Color.red;

        [Header("Buttons")]
        public Button backToLobbyButton;

        [Header("Player Stats")]
        public TextMeshProUGUI playerNameText;
        public TextMeshProUGUI ratingText;
        public TextMeshProUGUI winsText;
        public TextMeshProUGUI lossesText;

        private AuthManager authManager;

        private async void Start()
        {
            authManager = AuthManager.Instance;

            if (backToLobbyButton != null)
            {
                backToLobbyButton.onClick.AddListener(OnBackToLobby);
            }

            bool won = PlayerPrefs.GetInt("LastMatchWon", 0) == 1;

            DisplayResult(won);

            await authManager.GetProfileAsync();
            UpdatePlayerStats();
        }

        private void DisplayResult(bool won)
        {
            if (resultText != null)
            {
                resultText.text = won ? "VICTORY!" : "DEFEAT";
                resultText.color = won ? winColor : loseColor;
            }

            if (resultBackground != null)
            {
                Color bgColor = won ? winColor : loseColor;
                bgColor.a = 0.3f;
                resultBackground.color = bgColor;
            }

            if (eloChangeText != null)
            {
                eloChangeText.text = won ? "+25 ELO" : "-25 ELO";
                eloChangeText.color = won ? winColor : loseColor;
            }

            Logger.LogGame($"Match result: {(won ? "Victory" : "Defeat")}");
        }

        private void UpdatePlayerStats()
        {
            var user = authManager.CurrentUser;
            if (user == null) return;

            if (playerNameText != null)
            {
                playerNameText.text = user.username;
            }

            if (ratingText != null)
            {
                ratingText.text = $"Rating: {user.rating}";
            }

            if (winsText != null)
            {
                winsText.text = $"Wins: {user.wins}";
            }

            if (lossesText != null)
            {
                lossesText.text = $"Losses: {user.losses}";
            }
        }

        private void OnBackToLobby()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }
    }
}
