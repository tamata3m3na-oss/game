using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using PvpGame.Utils;
using PvpGame.Config;

namespace PvpGame.Auth
{
    [Serializable]
    public class RegisterRequest
    {
        public string email;
        public string username;
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

    [Serializable]
    public class AuthResponse
    {
        public UserData user;
        public string accessToken;
        public string refreshToken;
    }

    [Serializable]
    public class UserData
    {
        public int id;
        public string email;
        public string username;
        public int rating;
        public int wins;
        public int losses;
    }

    public class AuthManager : Singleton<AuthManager>
    {
        public UserData CurrentUser { get; private set; }
        public bool IsAuthenticated => tokenManager.HasTokens() && CurrentUser != null;

        private TokenManager tokenManager = new TokenManager();
        private GameConfig config;

        protected override void Awake()
        {
            base.Awake();
            config = GameConfig.Instance;
        }

        public async Task<(bool success, string error)> RegisterAsync(string email, string username, string password)
        {
            AppLogger.LogAuth($"Registering user: {email}");

            var request = new RegisterRequest
            {
                email = email,
                username = username,
                password = password
            };

            string json = JsonHelper.Serialize(request);
            var webRequest = new UnityWebRequest($"{config.restApiUrl}/auth/register", "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            var operation = webRequest.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var response = JsonHelper.Deserialize<AuthResponse>(webRequest.downloadHandler.text);
                if (response != null)
                {
                    tokenManager.AccessToken = response.accessToken;
                    tokenManager.RefreshToken = response.refreshToken;
                    CurrentUser = response.user;
                    AppLogger.LogSuccess($"Registration successful: {response.user.username}");
                    return (true, null);
                }
            }

            string error = webRequest.downloadHandler.text;
            AppLogger.LogError($"Registration failed: {error}");
            return (false, error);
        }

        public async Task<(bool success, string error)> LoginAsync(string email, string password)
        {
            AppLogger.LogAuth($"Logging in user: {email}");

            var request = new LoginRequest
            {
                email = email,
                password = password
            };

            string json = JsonHelper.Serialize(request);
            var webRequest = new UnityWebRequest($"{config.restApiUrl}/auth/login", "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            var operation = webRequest.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var response = JsonHelper.Deserialize<AuthResponse>(webRequest.downloadHandler.text);
                if (response != null)
                {
                    tokenManager.AccessToken = response.accessToken;
                    tokenManager.RefreshToken = response.refreshToken;
                    CurrentUser = response.user;
                    AppLogger.LogSuccess($"Login successful: {response.user.username}");
                    return (true, null);
                }
            }

            string error = webRequest.downloadHandler.text;
            AppLogger.LogError($"Login failed: {error}");
            return (false, error);
        }

        public async Task<bool> AutoLoginAsync()
        {
            if (!tokenManager.HasTokens())
            {
                AppLogger.LogAuth("No saved tokens found");
                return false;
            }

            AppLogger.LogAuth("Attempting auto-login with saved tokens");
            return await RefreshTokenAsync();
        }

        public async Task<bool> RefreshTokenAsync()
        {
            if (string.IsNullOrEmpty(tokenManager.RefreshToken))
            {
                AppLogger.LogWarning("No refresh token available");
                return false;
            }

            AppLogger.LogAuth("Refreshing access token");

            var request = new RefreshRequest
            {
                refreshToken = tokenManager.RefreshToken
            };

            string json = JsonHelper.Serialize(request);
            var webRequest = new UnityWebRequest($"{config.restApiUrl}/auth/refresh", "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            var operation = webRequest.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var response = JsonHelper.Deserialize<AuthResponse>(webRequest.downloadHandler.text);
                if (response != null)
                {
                    tokenManager.AccessToken = response.accessToken;
                    tokenManager.RefreshToken = response.refreshToken;
                    CurrentUser = response.user;
                    AppLogger.LogSuccess("Token refresh successful");
                    return true;
                }
            }

            AppLogger.LogError("Token refresh failed");
            tokenManager.ClearTokens();
            return false;
        }

        public async Task<bool> GetProfileAsync()
        {
            AppLogger.LogAuth("Fetching user profile");

            var webRequest = UnityWebRequest.Get($"{config.restApiUrl}/player/me");
            webRequest.SetRequestHeader("Authorization", $"Bearer {tokenManager.AccessToken}");

            var operation = webRequest.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var user = JsonHelper.Deserialize<UserData>(webRequest.downloadHandler.text);
                if (user != null)
                {
                    CurrentUser = user;
                    AppLogger.LogSuccess($"Profile loaded: {user.username}");
                    return true;
                }
            }

            AppLogger.LogError("Failed to fetch profile");
            return false;
        }

        public void Logout()
        {
            AppLogger.LogAuth("Logging out");
            tokenManager.ClearTokens();
            CurrentUser = null;
        }

        public string GetAccessToken()
        {
            return tokenManager.AccessToken;
        }
    }
}
