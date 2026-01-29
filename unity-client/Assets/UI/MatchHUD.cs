using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ShipBattle.Network;
using ShipBattle.Core;
using ShipBattle.Gameplay;
using System.Collections.Generic;

namespace ShipBattle.UI
{
    /// <summary>
    /// Main HUD for match gameplay.
    /// Displays player/opponent info, health bars, timer, and connection status.
    /// Phase 2 - New implementation.
    /// </summary>
    public class MatchHUD : MonoBehaviour
    {
        [Header("Player Info")]
        [SerializeField] private TextMeshProUGUI playerUsernameText;
        [SerializeField] private TextMeshProUGUI playerRatingText;
        [SerializeField] private Slider playerHealthBar;
        [SerializeField] private Slider playerShieldBar;

        [Header("Opponent Info")]
        [SerializeField] private TextMeshProUGUI opponentUsernameText;
        [SerializeField] private TextMeshProUGUI opponentRatingText;
        [SerializeField] private Slider opponentHealthBar;
        [SerializeField] private Slider opponentShieldBar;

        [Header("Match Info")]
        [SerializeField] private MatchTimer matchTimer;
        [SerializeField] private GameObject connectionIndicator;
        [SerializeField] private Image connectionStatusImage;
        [SerializeField] private Color connectedColor = Color.green;
        [SerializeField] private Color disconnectedColor = Color.red;

        [Header("Touch Controls (Android)")]
        [SerializeField] private GameObject touchControlsPanel;
        [SerializeField] private Button fireButton;
        [SerializeField] private Button abilityButton;

        private SocketClient socketClient;
        private SnapshotHandler snapshotHandler;
        private InputSender inputSender;

        private void Start()
        {
            socketClient = GameManager.Instance.SocketClient;
            
            // Subscribe to connection events
            socketClient.OnConnected += OnConnected;
            socketClient.OnDisconnected += OnDisconnected;
            socketClient.OnStateSnapshot += OnSnapshotReceived;

            // Setup touch controls for Android
            SetupTouchControls();

            // Initialize opponent info from GameManager
            SetOpponentInfo(
                GameManager.Instance.OpponentUsername,
                GameManager.Instance.OpponentRating
            );

            // Start timer
            if (matchTimer != null)
            {
                matchTimer.StartTimer();
            }

            UpdateConnectionStatus(socketClient.IsConnected);
        }

        private void SetupTouchControls()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            if (touchControlsPanel != null)
            {
                touchControlsPanel.SetActive(true);
            }
            #else
            if (touchControlsPanel != null)
            {
                touchControlsPanel.SetActive(false);
            }
            #endif

            // Setup button listeners
            if (fireButton != null)
            {
                fireButton.onClick.AddListener(OnFireButtonPressed);
            }

            if (abilityButton != null)
            {
                abilityButton.onClick.AddListener(OnAbilityButtonPressed);
            }

            // Get InputSender reference
            inputSender = FindObjectOfType<InputSender>();
        }

        private void OnSnapshotReceived(GameStateSnapshot snapshot)
        {
            if (snapshot?.ships == null || snapshot.ships.Count < 2)
            {
                return;
            }

            // Assuming ships[0] is player, ships[1] is opponent
            // (This should be determined by matching ship IDs with player IDs)
            ShipState playerShip = snapshot.ships[0];
            ShipState opponentShip = snapshot.ships[1];

            UpdatePlayerStats(playerShip.health, playerShip.shield);
            UpdateOpponentStats(opponentShip.health, opponentShip.shield);
        }

        private void UpdatePlayerStats(float health, float shield)
        {
            if (playerHealthBar != null)
            {
                playerHealthBar.value = health / 100f;
            }

            if (playerShieldBar != null)
            {
                playerShieldBar.value = shield / 100f;
            }
        }

        private void UpdateOpponentStats(float health, float shield)
        {
            if (opponentHealthBar != null)
            {
                opponentHealthBar.value = health / 100f;
            }

            if (opponentShieldBar != null)
            {
                opponentShieldBar.value = shield / 100f;
            }
        }

        private void SetOpponentInfo(string username, int rating)
        {
            if (opponentUsernameText != null)
            {
                opponentUsernameText.text = username;
            }

            if (opponentRatingText != null)
            {
                opponentRatingText.text = $"Rating: {rating}";
            }
        }

        private void OnConnected()
        {
            UpdateConnectionStatus(true);
        }

        private void OnDisconnected()
        {
            UpdateConnectionStatus(false);
        }

        private void UpdateConnectionStatus(bool isConnected)
        {
            if (connectionStatusImage != null)
            {
                connectionStatusImage.color = isConnected ? connectedColor : disconnectedColor;
            }

            if (connectionIndicator != null)
            {
                connectionIndicator.SetActive(!isConnected);
            }
        }

        // Touch control callbacks
        private void OnFireButtonPressed()
        {
            if (inputSender != null)
            {
                inputSender.OnFireButtonPressed();
            }
        }

        private void OnAbilityButtonPressed()
        {
            if (inputSender != null)
            {
                inputSender.OnAbilityButtonPressed();
            }
        }

        private void OnDestroy()
        {
            if (socketClient != null)
            {
                socketClient.OnConnected -= OnConnected;
                socketClient.OnDisconnected -= OnDisconnected;
                socketClient.OnStateSnapshot -= OnSnapshotReceived;
            }
        }
    }
}
