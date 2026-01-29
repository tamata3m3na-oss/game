using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;
using ShipBattle.Network;

public class SplashController : MonoBehaviour
{
    [SerializeField] private float splashDuration = 2f;
    
    private void Start()
    {
        StartCoroutine(SplashCoroutine());
    }
    
    private IEnumerator SplashCoroutine()
    {
        yield return new WaitForSeconds(splashDuration);
        
        AuthService.Instance.LoadTokens();
        string token = AuthService.Instance.GetAccessToken();
        
        if (!string.IsNullOrEmpty(token))
        {
            // Try auto-login
            _ = AutoLogin(token);
        }
        else
        {
            // No token, go to login
            SceneManager.LoadScene("Login");
        }
    }
    
    private async Task AutoLogin(string token)
    {
        try
        {
            // Connect to WebSocket using the stored token
            await SocketClient.Instance.ConnectAsync(token);
            SceneManager.LoadScene("Lobby");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SPLASH] Auto-login failed: {ex.Message}");
            SceneManager.LoadScene("Login");
        }
    }
}
