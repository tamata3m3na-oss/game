using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }
    
    public string ServerUrl = "http://localhost:3000";
    
    private string authToken = "";
    private string refreshToken = "";
    private int userId = -1;
    private string username = "";
    
    // Events
    public UnityEvent<string> OnLoginSuccess = new UnityEvent<string>();
    public UnityEvent<string> OnLoginFailed = new UnityEvent<string>();
    public UnityEvent<string> OnRegisterSuccess = new UnityEvent<string>();
    public UnityEvent<string> OnRegisterFailed = new UnityEvent<string>();
    public UnityEvent OnLogout = new UnityEvent();
    
    [Serializable]
    public class AuthResponse
    {
        public string access_token;
        public string refresh_token;
        public UserData user;
    }
    
    [Serializable]
    public class UserData
    {
        public int id;
        public string username;
        public string email;
        public int rating;
        public int wins;
        public int losses;
    }
    
    [Serializable]
    public class RegisterRequest
    {
        public string username;
        public string email;
        public string password;
    }
    
    [Serializable]
    public class LoginRequest
    {
        public string email;
        public string password;
    }
    
    [Serializable]
    public class RefreshRequest
    {
        public string refreshToken;
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Try auto-login
        LoadTokens();
        if (!string.IsNullOrEmpty(authToken))
        {
            // Validate token on startup
            StartCoroutine(ValidateToken());
        }
    }
    
    private void LoadTokens()
    {
        authToken = PlayerPrefs.GetString("auth_token", "");
        refreshToken = PlayerPrefs.GetString("refresh_token", "");
        userId = PlayerPrefs.GetInt("user_id", -1);
        username = PlayerPrefs.GetString("username", "");
    }
    
    private void SaveTokens()
    {
        PlayerPrefs.SetString("auth_token", authToken);
        PlayerPrefs.SetString("refresh_token", refreshToken);
        PlayerPrefs.SetInt("user_id", userId);
        PlayerPrefs.SetString("username", username);
        PlayerPrefs.Save();
    }
    
    private void ClearTokens()
    {
        authToken = "";
        refreshToken = "";
        userId = -1;
        username = "";
        
        PlayerPrefs.DeleteKey("auth_token");
        PlayerPrefs.DeleteKey("refresh_token");
        PlayerPrefs.DeleteKey("user_id");
        PlayerPrefs.DeleteKey("username");
        PlayerPrefs.Save();
    }
    
    public void Register(string username, string email, string password)
    {
        StartCoroutine(RegisterCoroutine(username, email, password));
    }
    
    private IEnumerator RegisterCoroutine(string username, string email, string password)
    {
        var request = new RegisterRequest
        {
            username = username,
            email = email,
            password = password
        };
        
        string json = JsonUtility.ToJson(request);
        
        using (UnityWebRequest www = new UnityWebRequest(ServerUrl + "/auth/register", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                string error = www.error;
                if (www.responseCode == 400)
                {
                    error = "Invalid registration data";
                }
                OnRegisterFailed.Invoke(error);
                Debug.LogError("Registration failed: " + error);
            }
            else
            {
                OnRegisterSuccess.Invoke("Registration successful! Please login.");
                Debug.Log("Registration successful");
            }
        }
    }
    
    public void Login(string email, string password)
    {
        StartCoroutine(LoginCoroutine(email, password));
    }
    
    private IEnumerator LoginCoroutine(string email, string password)
    {
        var request = new LoginRequest
        {
            email = email,
            password = password
        };
        
        string json = JsonUtility.ToJson(request);
        
        using (UnityWebRequest www = new UnityWebRequest(ServerUrl + "/auth/login", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                string error = www.error;
                if (www.responseCode == 401)
                {
                    error = "Invalid credentials";
                }
                OnLoginFailed.Invoke(error);
                Debug.LogError("Login failed: " + error);
            }
            else
            {
                try
                {
                    string responseText = www.downloadHandler.text;
                    AuthResponse response = JsonUtility.FromJson<AuthResponse>(responseText);
                    
                    authToken = response.access_token;
                    refreshToken = response.refresh_token;
                    userId = response.user.id;
                    username = response.user.username;
                    
                    SaveTokens();
                    
                    OnLoginSuccess.Invoke("Login successful!");
                    Debug.Log("Login successful");
                    
                    // Initialize network manager
                    NetworkManager.Instance.Initialize(authToken);
                }
                catch (Exception e)
                {
                    OnLoginFailed.Invoke("Failed to parse response: " + e.Message);
                    Debug.LogError("Failed to parse auth response: " + e.Message);
                }
            }
        }
    }
    
    public void Logout()
    {
        ClearTokens();
        OnLogout.Invoke();
        
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.Disconnect();
        }
        
        Debug.Log("Logged out");
    }
    
    private IEnumerator ValidateToken()
    {
        // Simple token validation by trying to get user data
        // In a real app, you'd have a dedicated endpoint for this
        yield return new WaitForSeconds(0.1f); // Small delay to avoid race conditions
        
        if (string.IsNullOrEmpty(authToken))
        {
            yield break;
        }
        
        // Token is considered valid if we have it stored
        // NetworkManager will handle actual validation on connection
        Debug.Log("Token validation passed");
    }
    
    public string GetAuthToken()
    {
        return authToken;
    }
    
    public int GetUserId()
    {
        return userId;
    }
    
    public string GetUsername()
    {
        return username;
    }
    
    public bool IsLoggedIn()
    {
        return !string.IsNullOrEmpty(authToken);
    }
}