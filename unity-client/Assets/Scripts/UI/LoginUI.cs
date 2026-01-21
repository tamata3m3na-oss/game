using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PvpGame.Auth;
using PvpGame.Utils;

namespace PvpGame.UI
{
    public class LoginUI : MonoBehaviour
    {
        [Header("UI References")]
        public TMP_InputField emailInput;
        public TMP_InputField usernameInput;
        public TMP_InputField passwordInput;
        public Button loginButton;
        public Button registerButton;
        public TextMeshProUGUI errorText;
        public GameObject loadingPanel;
        public GameObject registerPanel;

        private AuthManager authManager;
        private bool isRegisterMode = false;

        private async void Start()
        {
            authManager = AuthManager.Instance;

            loginButton.onClick.AddListener(OnLoginClicked);
            registerButton.onClick.AddListener(OnRegisterClicked);

            SetLoading(false);
            HideError();

            if (registerPanel != null)
            {
                registerPanel.SetActive(false);
            }

            AppLogger.LogAuth("Attempting auto-login");
            SetLoading(true);

            bool autoLoginSuccess = await authManager.AutoLoginAsync();

            SetLoading(false);

            if (autoLoginSuccess)
            {
                AppLogger.LogSuccess("Auto-login successful");
                LoadLobbyScene();
            }
            else
            {
                AppLogger.Log("No saved session, showing login screen");
            }
        }

        private async void OnLoginClicked()
        {
            string email = emailInput.text.Trim();
            string password = passwordInput.text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter email and password");
                return;
            }

            SetLoading(true);
            HideError();

            var result = await authManager.LoginAsync(email, password);

            SetLoading(false);

            if (result.success)
            {
                AppLogger.LogSuccess("Login successful");
                LoadLobbyScene();
            }
            else
            {
                ShowError(result.error ?? "Login failed");
            }
        }

        private async void OnRegisterClicked()
        {
            if (!isRegisterMode)
            {
                isRegisterMode = true;
                if (registerPanel != null)
                {
                    registerPanel.SetActive(true);
                }
                registerButton.GetComponentInChildren<TextMeshProUGUI>().text = "Create Account";
                return;
            }

            string email = emailInput.text.Trim();
            string username = usernameInput.text.Trim();
            string password = passwordInput.text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Please fill all fields");
                return;
            }

            SetLoading(true);
            HideError();

            var result = await authManager.RegisterAsync(email, username, password);

            SetLoading(false);

            if (result.success)
            {
                AppLogger.LogSuccess("Registration successful");
                LoadLobbyScene();
            }
            else
            {
                ShowError(result.error ?? "Registration failed");
            }
        }

        private void ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
                errorText.gameObject.SetActive(true);
            }
        }

        private void HideError()
        {
            if (errorText != null)
            {
                errorText.gameObject.SetActive(false);
            }
        }

        private void SetLoading(bool loading)
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(loading);
            }

            if (loginButton != null)
            {
                loginButton.interactable = !loading;
            }

            if (registerButton != null)
            {
                registerButton.interactable = !loading;
            }
        }

        private void LoadLobbyScene()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }
    }
}
