using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using BattleStar.Models;
using BattleStar.Network;

namespace BattleStar.Auth
{
    public class AuthManager : MonoBehaviour
    {
        private static AuthManager _instance;
        public static AuthManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AuthManager>();
                    if (_instance == null)
                    {
                        _instance = new GameObject("AuthManager").AddComponent<AuthManager>();
                        DontDestroyOnLoad(_instance.gameObject);
                    }
                }
                return _instance;
            }
        }

        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }
        public PlayerData CurrentPlayer { get; private set; }
        public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task<AuthResult> RegisterAsync(string name, string email, string password)
        {
            var request = new RegisterRequest(name, email, password);
            
            var result = await HttpService.Instance.PostAsync<AuthResponse>("auth/register", request);
            
            return HandleAuthResponse(result);
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            var request = new LoginRequest(email, password);
            
            var result = await HttpService.Instance.PostAsync<AuthResponse>("auth/login", request);
            
            return HandleAuthResponse(result);
        }

        private AuthResult HandleAuthResponse(HttpResult result)
        {
            if (result.success && result.data is AuthResponse authResponse)
            {
                StoreTokens(authResponse.accessToken, authResponse.refreshToken);
                CurrentPlayer = authResponse.player;
                return new AuthResult { success = true, data = authResponse, message = null };
            }
            
            return new AuthResult { success = false, data = null, message = result.error };
        }

        private void StoreTokens(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        public void Logout()
        {
            AccessToken = null;
            RefreshToken = null;
            CurrentPlayer = null;
        }

        public async Task<AuthResult> GetPlayerProfileAsync()
        {
            var result = await HttpService.Instance.GetAsync<AuthResponse>("player/me", AccessToken);
            
            if (result.success && result.data is AuthResponse authResponse)
            {
                CurrentPlayer = authResponse.player;
                return new AuthResult { success = true, data = authResponse, message = null };
            }
            
            return new AuthResult { success = false, data = null, message = result.error };
        }
    }

    public class AuthResult
    {
        public bool success;
        public object data;
        public string message;
    }
}