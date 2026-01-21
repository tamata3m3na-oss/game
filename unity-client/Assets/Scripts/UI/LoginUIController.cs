using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginUIController : MonoBehaviour
{
    [Header("Login Panel")]
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public Button loginButton;
    public TextMeshProUGUI loginErrorText;
    
    [Header("Register Panel")]
    public TMP_InputField registerUsernameInput;
    public TMP_InputField registerEmailInput;
    public TMP_InputField registerPasswordInput;
    public Button registerButton;
    public TextMeshProUGUI registerErrorText;
    public TextMeshProUGUI registerSuccessText;
    
    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject registerPanel;
    
    private void Start()
    {
        // Set up buttons
        if (loginButton != null)
        {
            loginButton.onClick.AddListener(OnLoginButtonClicked);
        }
        
        if (registerButton != null)
        {
            registerButton.onClick.AddListener(OnRegisterButtonClicked);
        }
        
        // Subscribe to auth events
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.OnLoginSuccess.AddListener(HandleLoginSuccess);
            AuthManager.Instance.OnLoginFailed.AddListener(HandleLoginFailed);
            AuthManager.Instance.OnRegisterSuccess.AddListener(HandleRegisterSuccess);
            AuthManager.Instance.OnRegisterFailed.AddListener(HandleRegisterFailed);
            
            // Check if already logged in
            if (AuthManager.Instance.IsLoggedIn())
            {
                // Auto-login
                SceneManager.LoadScene("LobbyScene");
            }
        }
        else
        {
            Debug.LogError("[LoginUIController] AuthManager.Instance is null. Bootstrap may have failed.");
        }
    }
    
    private void OnDestroy()
    {
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.OnLoginSuccess.RemoveListener(HandleLoginSuccess);
            AuthManager.Instance.OnLoginFailed.RemoveListener(HandleLoginFailed);
            AuthManager.Instance.OnRegisterSuccess.RemoveListener(HandleRegisterSuccess);
            AuthManager.Instance.OnRegisterFailed.RemoveListener(HandleRegisterFailed);
        }
    }
    
    private void OnLoginButtonClicked()
    {
        if (emailInputField == null || passwordInputField == null) return;
        
        string email = emailInputField.text;
        string password = passwordInputField.text;
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowLoginError("Please enter email and password");
            return;
        }
        
        ClearLoginError();
        AuthManager.Instance.Login(email, password);
    }
    
    private void OnRegisterButtonClicked()
    {
        if (registerUsernameInput == null || registerEmailInput == null || registerPasswordInput == null) return;
        
        string username = registerUsernameInput.text;
        string email = registerEmailInput.text;
        string password = registerPasswordInput.text;
        
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowRegisterError("Please fill all fields");
            return;
        }
        
        if (password.Length < 6)
        {
            ShowRegisterError("Password must be at least 6 characters");
            return;
        }
        
        ClearRegisterError();
        AuthManager.Instance.Register(username, email, password);
    }
    
    private void HandleLoginSuccess(string message)
    {
        Debug.Log("Login successful: " + message);
        SceneManager.LoadScene("LobbyScene");
    }
    
    private void HandleLoginFailed(string error)
    {
        ShowLoginError(error);
        Debug.LogError("Login failed: " + error);
    }
    
    private void HandleRegisterSuccess(string message)
    {
        ShowRegisterSuccess(message);
        Debug.Log("Registration successful: " + message);
        
        // Switch to login panel
        SwitchToLoginPanel();
    }
    
    private void HandleRegisterFailed(string error)
    {
        ShowRegisterError(error);
        Debug.LogError("Registration failed: " + error);
    }
    
    private void ShowLoginError(string error)
    {
        if (loginErrorText != null)
        {
            loginErrorText.text = error;
            loginErrorText.gameObject.SetActive(true);
        }
    }
    
    private void ClearLoginError()
    {
        if (loginErrorText != null)
        {
            loginErrorText.gameObject.SetActive(false);
        }
    }
    
    private void ShowRegisterError(string error)
    {
        if (registerErrorText != null)
        {
            registerErrorText.text = error;
            registerErrorText.gameObject.SetActive(true);
        }
    }
    
    private void ShowRegisterSuccess(string message)
    {
        if (registerSuccessText != null)
        {
            registerSuccessText.text = message;
            registerSuccessText.gameObject.SetActive(true);
        }
    }
    
    private void ClearRegisterError()
    {
        if (registerErrorText != null)
        {
            registerErrorText.gameObject.SetActive(false);
        }
        
        if (registerSuccessText != null)
        {
            registerSuccessText.gameObject.SetActive(false);
        }
    }
    
    public void SwitchToRegisterPanel()
    {
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(true);
        ClearLoginError();
        ClearRegisterError();
    }
    
    public void SwitchToLoginPanel()
    {
        if (registerPanel != null) registerPanel.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(true);
        ClearLoginError();
        ClearRegisterError();
    }
}