using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace ShipBattle.Network
{
    /// <summary>
    /// Handles authentication with the backend REST API.
    /// Enhanced with detailed logging.
    /// </summary>
    public class AuthService
    {
        private static AuthService instance;
        public static AuthService Instance
        {
            get
            {
                if (instance == null) 
                {
                    instance = new AuthService();
                    Debug.Log("[AuthService] Instance created");
                }
                return instance;
            }
        }

        private const string BASE_URL = "http://localhost:3000";
        private string accessToken;
        private string refreshToken;
        private UserData currentUser;

        public string AccessToken => accessToken;
        public string RefreshToken => refreshToken;
        public bool IsAuthenticated => !string.IsNullOrEmpty(accessToken);

        public string GetAccessToken() => accessToken;
        public int GetUserId() => currentUser?.id ?? 0;

        private AuthService()
        {
            Debug.Log($"[AuthService] Initialized with BASE_URL: {BASE_URL}");
        }

        public async Task<PlayerProfile> GetPlayerProfileAsync()
        {
            Debug.Log("[AuthService] Getting player profile...");
            if (currentUser != null)
            {
                Debug.Log($"[AuthService] Returning cached profile for: {currentUser.username}");
                return new PlayerProfile 
                { 
                    username = currentUser.username,
                    rating = currentUser.rating,
                    wins = currentUser.wins,
                    losses = currentUser.losses
                };
            }
            Debug.LogWarning("[AuthService] No current user data available");
            return null;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            Debug.Log($"[AuthService] LoginAsync called for: {email}");
            try
            {
                var result = await LoginAsyncInternal(email, password);
                Debug.Log($"[AuthService] Login result: {(result != null ? "Success" : "Failed")}");
                return result != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AuthService] LoginAsync exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RegisterAsync(string email, string password)
        {
            Debug.Log($"[AuthService] RegisterAsync called for: {email}");
            try 
            {
                var result = await RegisterAsyncInternal(email, email, password);
                Debug.Log($"[AuthService] Register result: {(result != null ? "Success" : "Failed")}");
                return result != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AuthService] RegisterAsync exception: {ex.Message}");
                return false;
            }
        }

        private async Task<AuthResponse> RegisterAsyncInternal(string email, string username, string password)
        {
            Debug.Log($"[AuthService] RegisterAsyncInternal called");
            var requestData = new
            {
                email = email,
                username = username,
                password = password
            };

            return await PostAuthRequestAsync($"{BASE_URL}/auth/register", requestData);
        }

        private async Task<AuthResponse> LoginAsyncInternal(string email, string password)
        {
            Debug.Log($"[AuthService] LoginAsyncInternal called");
            var requestData = new
            {
                email = email,
                password = password
            };

            return await PostAuthRequestAsync($"{BASE_URL}/auth/login", requestData);
        }

        public async Task<AuthResponse> RefreshTokenAsync()
        {
            Debug.Log("[AuthService] RefreshTokenAsync called");
            if (string.IsNullOrEmpty(refreshToken))
            {
                Debug.LogError("[AuthService] No refresh token available");
                throw new Exception("No refresh token available");
            }

            var requestData = new
            {
                refreshToken = refreshToken
            };

            return await PostAuthRequestAsync($"{BASE_URL}/auth/refresh", requestData);
        }

        private async Task<AuthResponse> PostAuthRequestAsync(string url, object data)
        {
            Debug.Log($"[AuthService] POST {url}");
            string json = JsonConvert.SerializeObject(data);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                Debug.Log($"[AuthService] Sending request...");
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Delay(10);
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    Debug.Log($"[AuthService] Response received: {responseText}");
                    AuthResponse response = JsonConvert.DeserializeObject<AuthResponse>(responseText);
                    
                    accessToken = response.accessToken;
                    refreshToken = response.refreshToken;
                    currentUser = response.user;
                    
                    Debug.Log($"[AuthService] User authenticated: {currentUser?.username} (ID: {currentUser?.id})");
                    SaveTokens();
                    
                    return response;
                }
                else
                {
                    string error = request.downloadHandler.text;
                    Debug.LogError($"[AuthService] Request failed: {error}");
                    throw new Exception($"Authentication failed: {error}");
                }
            }
        }

        private void SaveTokens()
        {
            Debug.Log("[AuthService] Saving tokens to PlayerPrefs");
            PlayerPrefs.SetString("AccessToken", accessToken);
            PlayerPrefs.SetString("RefreshToken", refreshToken);
            PlayerPrefs.Save();
            Debug.Log("[AuthService] Tokens saved successfully");
        }

        public void LoadTokens()
        {
            Debug.Log("[AuthService] Loading tokens from PlayerPrefs");
            accessToken = PlayerPrefs.GetString("AccessToken", "");
            refreshToken = PlayerPrefs.GetString("RefreshToken", "");
            bool hasToken = !string.IsNullOrEmpty(accessToken);
            Debug.Log($"[AuthService] Tokens loaded. Has access token: {hasToken}");
        }

        public void ClearTokens()
        {
            Debug.Log("[AuthService] Clearing tokens");
            accessToken = "";
            refreshToken = "";
            currentUser = null;
            PlayerPrefs.DeleteKey("AccessToken");
            PlayerPrefs.DeleteKey("RefreshToken");
            PlayerPrefs.Save();
            Debug.Log("[AuthService] Tokens cleared");
        }
    }

    [Serializable]
    public class AuthResponse
    {
        public UserData user { get; set; }
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
    }

    [Serializable]
    public class UserData
    {
        public int id { get; set; }
        public string email { get; set; }
        public string username { get; set; }
        public int rating { get; set; }
        public int wins { get; set; }
        public int losses { get; set; }
    }
}
