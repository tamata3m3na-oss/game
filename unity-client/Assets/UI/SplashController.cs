using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;
using ShipBattle.Network;
using TMPro;

public class SplashController : MonoBehaviour
{
    [SerializeField] private float splashDuration = 2f;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private TMP_Text subtitleText;
    
    private void Awake()
    {
        Debug.Log("[SPLASH] SplashController Awake");
        
        // Set default values if assigned
        if (titleText != null) titleText.text = "SHIP BATTLE";
        if (subtitleText != null) subtitleText.text = "Loading...";
        if (loadingIndicator != null) loadingIndicator.SetActive(true);
    }
    
    private void Start()
    {
        Debug.Log("[SPLASH] SplashController Start - Starting splash sequence");
        StartCoroutine(SplashCoroutine());
    }
    
    private IEnumerator SplashCoroutine()
    {
        Debug.Log($"[SPLASH] Waiting for {splashDuration} seconds...");
        yield return new WaitForSeconds(splashDuration);
        
        Debug.Log("[SPLASH] Loading tokens...");
        AuthService.Instance.LoadTokens();
        string token = AuthService.Instance.GetAccessToken();
        
        Debug.Log($"[SPLASH] Token check: {(string.IsNullOrEmpty(token) ? "No token found" : "Token exists")}");
        
        if (!string.IsNullOrEmpty(token))
        {
            // Try auto-login
            Debug.Log("[SPLASH] Attempting auto-login...");
            _ = AutoLogin(token);
        }
        else
        {
            // No token, go to login
            Debug.Log("[SPLASH] No token found, navigating to Login scene");
            SceneManager.LoadScene("Login");
        }
    }
    
    private async Task AutoLogin(string token)
    {
        try
        {
            Debug.Log("[SPLASH] Connecting to WebSocket with stored token...");
            await SocketClient.Instance.ConnectAsync(token);
            Debug.Log("[SPLASH] Auto-login successful, navigating to Lobby");
            SceneManager.LoadScene("Lobby");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SPLASH] Auto-login failed: {ex.Message}");
            Debug.Log("[SPLASH] Navigating to Login scene due to auto-login failure");
            SceneManager.LoadScene("Login");
        }
    }
}
