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
    
    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
        registerButton.onClick.AddListener(OnRegisterClicked);
    }
    
    private void OnLoginClicked()
    {
        if (isProcessing) return;
        
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Please enter email and password", Color.red);
            return;
        }
        
        _ = TryLogin(email, password);
    }
    
    private void OnRegisterClicked()
    {
        if (isProcessing) return;
        
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Please enter email and password", Color.red);
            return;
        }
        
        _ = TryRegister(email, password);
    }
    
    private async Task TryLogin(string email, string password)
    {
        isProcessing = true;
        loginButton.interactable = false;
        registerButton.interactable = false;
        
        ShowStatus("Logging in...", Color.yellow);
        
        try
        {
            var response = await AuthService.Instance.LoginAsync(email, password);
            
            if (response != null)
            {
                ShowStatus("Login successful! Connecting...", Color.green);
                await Task.Delay(1000);
                
                // Connect to WebSocket
                await SocketClient.Instance.ConnectAsync(AuthService.Instance.GetAccessToken());
                
                SceneManager.LoadScene("Lobby");
            }
            else
            {
                ShowStatus("Login failed. Check credentials.", Color.red);
            }
        }
        catch (System.Exception ex)
        {
            ShowStatus($"Error: {ex.Message}", Color.red);
            Debug.LogError($"[LOGIN] Error: {ex}");
        }
        finally
        {
            isProcessing = false;
            loginButton.interactable = true;
            registerButton.interactable = true;
        }
    }
    
    private async Task TryRegister(string email, string password)
    {
        isProcessing = true;
        loginButton.interactable = false;
        registerButton.interactable = false;
        
        ShowStatus("Registering...", Color.yellow);
        
        try
        {
            // Using email as username
            var response = await AuthService.Instance.RegisterAsync(email, email, password);
            
            if (response != null)
            {
                ShowStatus("Registration successful! Logging in...", Color.green);
                await Task.Delay(1000);
                
                // Auto login after registration
                await TryLogin(email, password);
            }
            else
            {
                ShowStatus("Registration failed. Email may exist.", Color.red);
            }
        }
        catch (System.Exception ex)
        {
            ShowStatus($"Error: {ex.Message}", Color.red);
            Debug.LogError($"[REGISTER] Error: {ex}");
        }
        finally
        {
            isProcessing = false;
            loginButton.interactable = true;
            registerButton.interactable = true;
        }
    }
    
    private void ShowStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
    }
}
