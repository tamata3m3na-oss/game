using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using ShipBattle.Network;
using TMPro;

public class LoginController : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private TMP_Text errorMessageText;

    [Header("Labels")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text usernameLabel;
    [SerializeField] private TMP_Text passwordLabel;
    
    private bool isProcessing = false;
    
    private void Awake()
    {
        Debug.Log("[LOGIN] LoginController Awake");
        if (titleText != null) titleText.text = "Login";
        if (usernameLabel != null) usernameLabel.text = "Username";
        if (passwordLabel != null) passwordLabel.text = "Password";
        if (errorMessageText != null) errorMessageText.gameObject.SetActive(false); // Hidden by default
    }
    
    private void Start()
    {
        Debug.Log("[LOGIN] LoginController Start - Setting up button listeners");
        
        if (loginButton != null)
        {
            loginButton.onClick.AddListener(Login);
            Debug.Log("[LOGIN] Login button listener added");
        }
        else
        {
            Debug.LogError("[LOGIN] Login button is not assigned!");
        }
        
        if (registerButton != null)
        {
            registerButton.onClick.AddListener(Register);
            Debug.Log("[LOGIN] Register button listener added");
        }
        else
        {
            Debug.LogError("[LOGIN] Register button is not assigned!");
        }
        
        if (usernameInput == null)
            Debug.LogError("[LOGIN] Username input is not assigned!");
        if (passwordInput == null)
            Debug.LogError("[LOGIN] Password input is not assigned!");
        if (errorMessageText == null)
            Debug.LogError("[LOGIN] Error message text is not assigned!");
        
        ShowStatus("Enter your username and password", Color.white);
    }
    
    private void OnDestroy()
    {
        Debug.Log("[LOGIN] LoginController OnDestroy - Removing listeners");
        if (loginButton != null)
            loginButton.onClick.RemoveListener(Login);
        if (registerButton != null)
            registerButton.onClick.RemoveListener(Register);
    }
    
    public void Login()
    {
        Debug.Log("[LOGIN] Login called");
        
        if (isProcessing) 
        {
            Debug.Log("[LOGIN] Already processing, ignoring");
            return;
        }
        
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();
        
        Debug.Log($"[LOGIN] Username entered: {username}");
        
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Please enter username and password", Color.red);
            Debug.LogWarning("[LOGIN] Validation failed: Empty username or password");
            return;
        }
        
        _ = TryLogin(username, password);
    }
    
    public void Register()
    {
        Debug.Log("[LOGIN] Register called");
        
        if (isProcessing) 
        {
            Debug.Log("[LOGIN] Already processing, ignoring");
            return;
        }
        
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();
        
        Debug.Log($"[LOGIN] Username entered: {username}");
        
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Please enter username and password", Color.red);
            Debug.LogWarning("[LOGIN] Validation failed: Empty username or password");
            return;
        }
        
        _ = TryRegister(username, password);
    }
    
    private async Task TryLogin(string username, string password)
    {
        Debug.Log($"[LOGIN] TryLogin started for: {username}");
        isProcessing = true;
        SetUIInteractable(false);
        
        ShowStatus("Logging in...", Color.yellow);
        
        try
        {
            var response = await AuthService.Instance.LoginAsync(username, password);
            
            if (response)
            {
                Debug.Log("[LOGIN] Login successful");
                ShowStatus("Login successful! Connecting...", Color.green);
                await Task.Delay(500);
                
                // Connect to WebSocket
                await SocketClient.Instance.ConnectAsync(AuthService.Instance.GetAccessToken());
                
                Debug.Log("[LOGIN] WebSocket connected, navigating to Lobby");
                SceneManager.LoadScene("Lobby");
            }
            else
            {
                ShowStatus("Login failed. Check credentials.", Color.red);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LOGIN] Error: {ex.Message}");
            ShowStatus($"Error: {ex.Message}", Color.red);
        }
        finally
        {
            isProcessing = false;
            SetUIInteractable(true);
        }
    }
    
    private async Task TryRegister(string username, string password)
    {
        Debug.Log($"[LOGIN] TryRegister started for: {username}");
        isProcessing = true;
        SetUIInteractable(false);
        
        ShowStatus("Registering...", Color.yellow);
        
        try
        {
            var response = await AuthService.Instance.RegisterAsync(username, password);
            
            if (response)
            {
                Debug.Log("[LOGIN] Registration successful");
                ShowStatus("Registration successful! Logging in...", Color.green);
                await Task.Delay(500);
                await TryLogin(username, password);
            }
            else
            {
                ShowStatus("Registration failed. Username may exist.", Color.red);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LOGIN] Error: {ex.Message}");
            ShowStatus($"Error: {ex.Message}", Color.red);
        }
        finally
        {
            isProcessing = false;
            SetUIInteractable(true);
        }
    }
    
    private void SetUIInteractable(bool interactable)
    {
        if (loginButton != null) loginButton.interactable = interactable;
        if (registerButton != null) registerButton.interactable = interactable;
        if (usernameInput != null) usernameInput.interactable = interactable;
        if (passwordInput != null) passwordInput.interactable = interactable;
    }
    
    private void ShowStatus(string message, Color color)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.color = color;
            errorMessageText.gameObject.SetActive(true);
            Debug.Log($"[LOGIN] Status: {message}");
        }
    }
}
