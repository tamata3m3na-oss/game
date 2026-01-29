using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using ShipBattle.Network;

public class LoginController : MonoBehaviour
{
    [SerializeField] private InputField emailInput;
    [SerializeField] private InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private Text statusText;
    
    private bool isProcessing = false;
    
    private void Awake()
    {
        Debug.Log("[LOGIN] LoginController Awake");
    }
    
    private void Start()
    {
        Debug.Log("[LOGIN] LoginController Start - Setting up button listeners");
        
        if (loginButton != null)
        {
            loginButton.onClick.AddListener(OnLoginClicked);
            Debug.Log("[LOGIN] Login button listener added");
        }
        else
        {
            Debug.LogError("[LOGIN] Login button is not assigned!");
        }
        
        if (registerButton != null)
        {
            registerButton.onClick.AddListener(OnRegisterClicked);
            Debug.Log("[LOGIN] Register button listener added");
        }
        else
        {
            Debug.LogError("[LOGIN] Register button is not assigned!");
        }
        
        if (emailInput == null)
            Debug.LogError("[LOGIN] Email input is not assigned!");
        if (passwordInput == null)
            Debug.LogError("[LOGIN] Password input is not assigned!");
        if (statusText == null)
            Debug.LogError("[LOGIN] Status text is not assigned!");
        
        ShowStatus("Enter your email and password", Color.white);
    }
    
    private void OnDestroy()
    {
        Debug.Log("[LOGIN] LoginController OnDestroy - Removing listeners");
        if (loginButton != null)
            loginButton.onClick.RemoveListener(OnLoginClicked);
        if (registerButton != null)
            registerButton.onClick.RemoveListener(OnRegisterClicked);
    }
    
    private void OnLoginClicked()
    {
        Debug.Log("[LOGIN] Login button clicked");
        
        if (isProcessing) 
        {
            Debug.Log("[LOGIN] Already processing, ignoring click");
            return;
        }
        
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();
        
        Debug.Log($"[LOGIN] Email entered: {(string.IsNullOrEmpty(email) ? "EMPTY" : email)}");
        Debug.Log($"[LOGIN] Password entered: {(string.IsNullOrEmpty(password) ? "EMPTY" : "***")}");
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Please enter email and password", Color.red);
            Debug.LogWarning("[LOGIN] Validation failed: Empty email or password");
            return;
        }
        
        _ = TryLogin(email, password);
    }
    
    private void OnRegisterClicked()
    {
        Debug.Log("[LOGIN] Register button clicked");
        
        if (isProcessing) 
        {
            Debug.Log("[LOGIN] Already processing, ignoring click");
            return;
        }
        
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();
        
        Debug.Log($"[LOGIN] Email entered: {(string.IsNullOrEmpty(email) ? "EMPTY" : email)}");
        Debug.Log($"[LOGIN] Password entered: {(string.IsNullOrEmpty(password) ? "EMPTY" : "***")}");
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Please enter email and password", Color.red);
            Debug.LogWarning("[LOGIN] Validation failed: Empty email or password");
            return;
        }
        
        _ = TryRegister(email, password);
    }
    
    private async Task TryLogin(string email, string password)
    {
        Debug.Log($"[LOGIN] TryLogin started for: {email}");
        isProcessing = true;
        loginButton.interactable = false;
        registerButton.interactable = false;
        
        ShowStatus("Logging in...", Color.yellow);
        
        try
        {
            var response = await AuthService.Instance.LoginAsync(email, password);
            
            if (response)
            {
                Debug.Log("[LOGIN] Login API call successful");
                ShowStatus("Login successful! Connecting...", Color.green);
                await Task.Delay(1000);
                
                // Connect to WebSocket
                Debug.Log("[LOGIN] Connecting to WebSocket...");
                await SocketClient.Instance.ConnectAsync(AuthService.Instance.GetAccessToken());
                
                Debug.Log("[LOGIN] WebSocket connected, navigating to Lobby");
                SceneManager.LoadScene("Lobby");
            }
            else
            {
                Debug.LogWarning("[LOGIN] Login API returned false");
                ShowStatus("Login failed. Check credentials.", Color.red);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LOGIN] Error during login: {ex.Message}");
            Debug.LogError($"[LOGIN] Stack trace: {ex.StackTrace}");
            ShowStatus($"Error: {ex.Message}", Color.red);
        }
        finally
        {
            isProcessing = false;
            loginButton.interactable = true;
            registerButton.interactable = true;
            Debug.Log("[LOGIN] TryLogin completed");
        }
    }
    
    private async Task TryRegister(string email, string password)
    {
        Debug.Log($"[LOGIN] TryRegister started for: {email}");
        isProcessing = true;
        loginButton.interactable = false;
        registerButton.interactable = false;
        
        ShowStatus("Registering...", Color.yellow);
        
        try
        {
            // Using email as username
            var response = await AuthService.Instance.RegisterAsync(email, password);
            
            if (response)
            {
                Debug.Log("[LOGIN] Registration successful");
                ShowStatus("Registration successful! Logging in...", Color.green);
                await Task.Delay(1000);
                
                // Auto login after registration
                Debug.Log("[LOGIN] Auto-logging in after registration...");
                await TryLogin(email, password);
            }
            else
            {
                Debug.LogWarning("[LOGIN] Registration API returned false");
                ShowStatus("Registration failed. Email may exist.", Color.red);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LOGIN] Error during registration: {ex.Message}");
            Debug.LogError($"[LOGIN] Stack trace: {ex.StackTrace}");
            ShowStatus($"Error: {ex.Message}", Color.red);
        }
        finally
        {
            isProcessing = false;
            loginButton.interactable = true;
            registerButton.interactable = true;
            Debug.Log("[LOGIN] TryRegister completed");
        }
    }
    
    private void ShowStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
            Debug.Log($"[LOGIN] Status: {message}");
        }
        else
        {
            Debug.LogWarning($"[LOGIN] Cannot show status (text component null): {message}");
        }
    }
}
