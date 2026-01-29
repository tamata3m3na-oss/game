using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using BattleStar.Auth;
using BattleStar.Core;
using TMPro;

namespace BattleStar.UI
{
    public class LoginController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup loginPanel;
        [SerializeField] private CanvasGroup registerPanel;

        [SerializeField] private TMP_InputField loginEmailField;
        [SerializeField] private TMP_InputField loginPasswordField;
        [SerializeField] private Button loginButton;

        [SerializeField] private TMP_InputField registerNameField;
        [SerializeField] private TMP_InputField registerEmailField;
        [SerializeField] private TMP_InputField registerPasswordField;
        [SerializeField] private Button registerButton;

        [SerializeField] private Button switchToLoginButton;
        [SerializeField] private Button switchToRegisterButton;

        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private GameObject loadingIndicator;

        private bool isProcessing;

        private void Start()
        {
            InitializeUI();
            ShowLoginPanel();

            if (AuthManager.Instance.IsAuthenticated)
            {
                OnLoginSuccess();
            }
        }

        private void InitializeUI()
        {
            loginButton.onClick.AddListener(OnLoginClicked);
            registerButton.onClick.AddListener(OnRegisterClicked);
            switchToLoginButton.onClick.AddListener(ShowLoginPanel);
            switchToRegisterButton.onClick.AddListener(ShowRegisterPanel);

            loginEmailField.onValueChanged.AddListener(ClearError);
            loginPasswordField.onValueChanged.AddListener(ClearError);
            registerNameField.onValueChanged.AddListener(ClearError);
            registerEmailField.onValueChanged.AddListener(ClearError);
            registerPasswordField.onValueChanged.AddListener(ClearError);
        }

        private void ShowLoginPanel()
        {
            ShowPanel(loginPanel);
            HidePanel(registerPanel);
            ClearError("");
        }

        private void ShowRegisterPanel()
        {
            ShowPanel(registerPanel);
            HidePanel(loginPanel);
            ClearError("");
        }

        private void ShowPanel(CanvasGroup panel)
        {
            panel.alpha = 1;
            panel.interactable = true;
            panel.blocksRaycasts = true;
        }

        private void HidePanel(CanvasGroup panel)
        {
            panel.alpha = 0;
            panel.interactable = false;
            panel.blocksRaycasts = false;
        }

        private async void OnLoginClicked()
        {
            if (isProcessing) return;

            if (string.IsNullOrEmpty(loginEmailField.text))
            {
                ShowError("Please enter your email");
                return;
            }

            if (string.IsNullOrEmpty(loginPasswordField.text))
            {
                ShowError("Please enter your password");
                return;
            }

            await ProcessLogin();
        }

        private async Task ProcessLogin()
        {
            SetProcessing(true);

            try
            {
                var result = await AuthManager.Instance.LoginAsync(
                    loginEmailField.text,
                    loginPasswordField.text
                );

                if (result.success)
                {
                    OnLoginSuccess();
                }
                else
                {
                    ShowError(result.message ?? "Login failed. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LoginController] Login error: {ex.Message}");
                ShowError("An unexpected error occurred. Please try again.");
            }
            finally
            {
                SetProcessing(false);
            }
        }

        private void OnLoginSuccess()
        {
            ClearError("");
            LoginAnalytics();
            GameManager.Instance.HandleLoginSuccess();
        }

        private void LoginAnalytics()
        {
            Debug.Log($"[LoginController] User logged in: {loginEmailField.text}");
        }

        private async void OnRegisterClicked()
        {
            if (isProcessing) return;

            if (string.IsNullOrEmpty(registerNameField.text))
            {
                ShowError("Please enter your username");
                return;
            }

            if (string.IsNullOrEmpty(registerEmailField.text))
            {
                ShowError("Please enter your email");
                return;
            }

            if (string.IsNullOrEmpty(registerPasswordField.text))
            {
                ShowError("Please enter a password");
                return;
            }

            await ProcessRegister();
        }

        private async Task ProcessRegister()
        {
            SetProcessing(true);

            try
            {
                var result = await AuthManager.Instance.RegisterAsync(
                    registerEmailField.text,
                    registerNameField.text,
                    registerPasswordField.text
                );

                if (result.success)
                {
                    ShowSuccess("Registration successful! Loading...");
                    await Task.Delay(1000);
                    OnLoginSuccess();
                }
                else
                {
                    ShowError(result.message ?? "Registration failed. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LoginController] Registration error: {ex.Message}");
                ShowError("An unexpected error occurred. Please try again.");
            }
            finally
            {
                SetProcessing(false);
            }
        }

        private void SetProcessing(bool processing)
        {
            isProcessing = processing;
            
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(processing);
            }

            loginButton.interactable = !processing;
            registerButton.interactable = !processing;
            switchToLoginButton.interactable = !processing;
            switchToRegisterButton.interactable = !processing;

            loginEmailField.interactable = !processing;
            loginPasswordField.interactable = !processing;
            registerNameField.interactable = !processing;
            registerEmailField.interactable = !processing;
            registerPasswordField.interactable = !processing;
        }

        private void ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
                errorText.color = Color.red;
            }
            Debug.LogWarning($"[LoginController] Error: {message}");
        }

        private void ShowSuccess(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
                errorText.color = Color.green;
            }
            Debug.Log($"[LoginController] Success: {message}");
        }

        private void ClearError(string value)
        {
            if (errorText != null && errorText.text.Length > 0)
            {
                errorText.text = string.Empty;
            }
        }

        private void OnDestroy()
        {
            loginButton?.onClick.RemoveAllListeners();
            registerButton?.onClick.RemoveAllListeners();
            switchToLoginButton?.onClick.RemoveAllListeners();
            switchToRegisterButton?.onClick.RemoveAllListeners();
        }
    }
}