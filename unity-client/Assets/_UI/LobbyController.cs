using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BattleStar.Auth;
using BattleStar.Core;

namespace BattleStar.UI
{
    public class LobbyController : MonoBehaviour
    {
        [Header("Welcome UI")]
        [SerializeField] private TextMeshProUGUI welcomeText;
        
        [Header("Player Stats UI")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI ratingText;
        [SerializeField] private TextMeshProUGUI winsText;
        [SerializeField] private TextMeshProUGUI lossesText;
        [SerializeField] private TextMeshProUGUI drawsText;
        [SerializeField] private TextMeshProUGUI totalMatchesText;
        
        [Header("Action Buttons")]
        [SerializeField] private Button logoutButton;
        
        [Header("Loading UI")]
        [SerializeField] private GameObject loadingIndicator;
        
        private void Start()
        {
            Debug.Log("[LobbyController] Initializing lobby...");
            
            InitializeUI();
            DisplayPlayerData();
            
            Debug.Log("[LobbyController] Lobby initialized successfully");
        }
        
        private void InitializeUI()
        {
            if (logoutButton != null)
            {
                logoutButton.onClick.AddListener(OnLogoutClicked);
            }
            
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(false);
            }
            
            ClearErrorMessages();
        }
        
        private void DisplayPlayerData()
        {
            var player = AuthManager.Instance.CurrentPlayer;
            
            if (player == null)
            {
                ShowError("No player data available");
                Debug.LogError("[LobbyController] CurrentPlayer is null");
                return;
            }
            
            // Update welcome message
            if (welcomeText != null)
            {
                welcomeText.text = $"Welcome, <b>{player.name}</b>!";
                welcomeText.color = Color.green;
            }
            
            // Update player details
            if (playerNameText != null)
            {
                playerNameText.text = $"Player: {player.name}";
            }
            
            if (ratingText != null)
            {
                ratingText.text = $"Rating: <b>{player.rating}</b>";
            }
            
            if (winsText != null)
            {
                winsText.text = $"Wins: {player.wins}";
            }
            
            if (lossesText != null)
            {
                lossesText.text = $"Losses: {player.losses}";
            }
            
            if (drawsText != null)
            {
                drawsText.text = $"Draws: {player.draws}";
            }
            
            if (totalMatchesText != null)
            {
                totalMatchesText.text = $"Total Matches: {player.totalMatches}";
            }
            
            Debug.Log($"[LobbyController] Player data displayed: {player.name} (Rating: {player.rating})");
            
            // Analytics tracking
            TrackLobbyEntry(player.name, player.rating);
        }
        
        private void TrackLobbyEntry(string playerName, int rating)
        {
            Debug.Log($"[LobbyController] Player {playerName} entered lobby with rating {rating}");
        }
        
        private async void OnLogoutClicked()
        {
            Debug.Log("[LobbyController] Logout button clicked");
            
            SetProcessing(true);
            
            try
            {
                // Clear player session
                var playerName = AuthManager.Instance.CurrentPlayer?.name ?? "Unknown";
                
                AuthManager.Instance.Logout();
                
                Debug.Log($"[LobbyController] Player {playerName} logged out successfully");
                
                // Return to login scene
                await GameManager.Instance.NavigateToLogin();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LobbyController] Logout error: {ex.Message}");
                ShowError("An error occurred during logout");
                SetProcessing(false);
            }
        }
        
        private void SetProcessing(bool processing)
        {
            if (logoutButton != null)
            {
                logoutButton.interactable = !processing;
            }
            
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(processing);
            }
        }
        
        private void ShowError(string message)
        {
            // For Phase 1, we'll just log errors
            // Phase 2 would add a proper error display UI
            Debug.LogError($"[LobbyController] {message}");
        }
        
        private void ClearErrorMessages()
        {
            // Clear any error states
        }
        
        private void OnDestroy()
        {
            logoutButton?.onClick.RemoveAllListeners();
        }
        
        // Phase 2 preparation methods (not implemented yet)
        /*
        public void StartMatchmaking()
        {
            // Will be implemented in Phase 2
        }
        
        public void ShowLeaderboard()
        {
            // Will be implemented in Phase 2
        }
        
        public void ShowSettings()
        {
            // Will be implemented in Phase 2
        }
        */
    }
}