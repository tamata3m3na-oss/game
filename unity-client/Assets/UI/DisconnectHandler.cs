using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ShipBattle.Network;
using ShipBattle.Core;

namespace ShipBattle.UI
{
    /// <summary>
    /// Handles disconnection and automatic reconnection logic.
    /// Shows reconnection UI and attempts to reconnect within timeout.
    /// Phase 2 - New implementation.
    /// </summary>
    public class DisconnectHandler : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private GameObject reconnectPanel;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button cancelButton;

        [Header("Configuration")]
        [SerializeField] private float reconnectTimeout = 15f;
        [SerializeField] private float retryInterval = 2f;
        [SerializeField] private int maxRetries = 5;

        private SocketClient socketClient;
        private AuthService authService;
        private bool isReconnecting;
        private float reconnectTimer;
        private int retryCount;

        private void Start()
        {
            socketClient = GameManager.Instance.SocketClient;
            authService = AuthService.Instance;
            authService.LoadTokens();

            // Subscribe to disconnect event
            socketClient.OnDisconnected += OnDisconnected;

            // Setup UI
            if (reconnectPanel != null)
            {
                reconnectPanel.SetActive(false);
            }

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(OnRetryButtonPressed);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelButtonPressed);
            }
        }

        private void Update()
        {
            if (isReconnecting)
            {
                reconnectTimer += Time.deltaTime;

                if (reconnectTimer >= reconnectTimeout)
                {
                    Debug.LogError("[DisconnectHandler] Reconnection timeout");
                    OnReconnectFailed();
                }
            }
        }

        private void OnDisconnected()
        {
            Debug.LogWarning("[DisconnectHandler] Connection lost, starting reconnection...");
            StartReconnection();
        }

        private void StartReconnection()
        {
            if (isReconnecting)
            {
                return;
            }

            isReconnecting = true;
            reconnectTimer = 0f;
            retryCount = 0;

            if (reconnectPanel != null)
            {
                reconnectPanel.SetActive(true);
            }

            UpdateStatusText("Connection lost. Reconnecting...");

            // Start reconnection attempts
            _ = AttemptReconnect();
        }

        private async Task AttemptReconnect()
        {
            while (isReconnecting && retryCount < maxRetries)
            {
                retryCount++;
                UpdateStatusText($"Reconnecting... (Attempt {retryCount}/{maxRetries})");

                try
                {
                    // Try to reconnect with existing token
                    await socketClient.ConnectAsync(authService.AccessToken);

                    Debug.Log("[DisconnectHandler] Reconnection successful");
                    OnReconnectSuccess();
                    return;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[DisconnectHandler] Reconnection attempt {retryCount} failed: {e.Message}");
                }

                // Wait before next retry
                await Task.Delay((int)(retryInterval * 1000));

                // Check if timeout exceeded
                if (reconnectTimer >= reconnectTimeout)
                {
                    break;
                }
            }

            // All retries failed
            OnReconnectFailed();
        }

        private void OnReconnectSuccess()
        {
            isReconnecting = false;
            reconnectTimer = 0f;

            if (reconnectPanel != null)
            {
                reconnectPanel.SetActive(false);
            }

            Debug.Log("[DisconnectHandler] Successfully reconnected");
        }

        private void OnReconnectFailed()
        {
            isReconnecting = false;
            UpdateStatusText("Reconnection failed. Please return to lobby.");

            if (retryButton != null)
            {
                retryButton.gameObject.SetActive(false);
            }

            if (cancelButton != null)
            {
                var buttonText = cancelButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Return to Lobby";
                }
            }
        }

        private void OnRetryButtonPressed()
        {
            Debug.Log("[DisconnectHandler] Manual retry requested");
            retryCount = 0;
            reconnectTimer = 0f;
            _ = AttemptReconnect();
        }

        private void OnCancelButtonPressed()
        {
            Debug.Log("[DisconnectHandler] User cancelled reconnection");
            isReconnecting = false;

            // Return to lobby
            GameManager.Instance.LoadLobby();
        }

        private void UpdateStatusText(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private void OnDestroy()
        {
            if (socketClient != null)
            {
                socketClient.OnDisconnected -= OnDisconnected;
            }
        }
    }
}
