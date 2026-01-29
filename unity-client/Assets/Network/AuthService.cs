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
    /// Do not modify - Phase 1 completed code.
    /// </summary>
    public class AuthService
    {
        private static AuthService instance;
        public static AuthService Instance
        {
            get
            {
                if (instance == null) instance = new AuthService();
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

        public async Task<UserData> GetPlayerProfileAsync()
        {
             if (currentUser == null)
             {
                 // Ideally fetch from server if null, but for now return cached or throw
                 if (IsAuthenticated)
                 {
                      // We could implement a fetch here, but let's assume it was set during login
                      // Or if we need to fetch: await FetchProfile();
                 }
             }
             return currentUser;
        }

        public async Task<bool> RegisterAsync(string email, string password)
        {
            // Using email as username for now as per UI limitation
            try 
            {
                var result = await RegisterAsync(email, email, password);
                return result != null;
            }
            catch { return false; }
        }

        public async Task<bool> LoginAsyncWrapper(string email, string password)
        {
             try
             {
                 var result = await LoginAsync(email, password);
                 return result != null;
             }
             catch { return false; }
        }

        public async Task<AuthResponse> RegisterAsync(string email, string username, string password)
        {
            var requestData = new
            {
                email = email,
                username = username,
                password = password
            };

            return await PostAuthRequestAsync($"{BASE_URL}/auth/register", requestData);
        }

        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            var requestData = new
            {
                email = email,
                password = password
            };

            return await PostAuthRequestAsync($"{BASE_URL}/auth/login", requestData);
        }

        public async Task<AuthResponse> RefreshTokenAsync()
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
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
            string json = JsonConvert.SerializeObject(data);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Delay(10);
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    AuthResponse response = JsonConvert.DeserializeObject<AuthResponse>(responseText);
                    
                    accessToken = response.accessToken;
                    refreshToken = response.refreshToken;
                    currentUser = response.user;
                    
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
            PlayerPrefs.SetString("AccessToken", accessToken);
            PlayerPrefs.SetString("RefreshToken", refreshToken);
            PlayerPrefs.Save();
        }

        public void LoadTokens()
        {
            accessToken = PlayerPrefs.GetString("AccessToken", "");
            refreshToken = PlayerPrefs.GetString("RefreshToken", "");
        }

        public void ClearTokens()
        {
            accessToken = "";
            refreshToken = "";
            PlayerPrefs.DeleteKey("AccessToken");
            PlayerPrefs.DeleteKey("RefreshToken");
            PlayerPrefs.Save();
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
