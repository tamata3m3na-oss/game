using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using BattleStar.Auth;
using BattleStar.UI;
using BattleStar.Network;

namespace BattleStar.Core
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                    if (_instance == null)
                    {
                        _instance = new GameObject("GameManager").AddComponent<GameManager>();
                        DontDestroyOnLoad(_instance.gameObject);
                    }
                }
                return _instance;
            }
        }

        [Header("Game Settings")]
        [SerializeField] private bool debugMode = false;

        [Header("Scenes")]
        [SerializeField] private string loginScene = "Login";
        [SerializeField] private string lobbyScene = "Lobby";

        public bool IsInitialized => HttpService.Instance != null && AuthManager.Instance != null;

        public event Action OnAuthSuccess;
        public event Action OnAuthFailed;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeGame();
        }

        private void InitializeGame()
        {
            Debug.Log("[GameManager] Initializing game...");
            
            EnsureRequiredServices();
            
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            Debug.Log("[GameManager] Game initialized successfully");
        }

        private void EnsureRequiredServices()
        {
            if (FindObjectOfType<HttpService>() == null)
            {
                var httpGO = new GameObject("HttpService");
                httpGO.AddComponent<HttpService>();
                DontDestroyOnLoad(httpGO);
                Debug.Log("[GameManager] HttpService created");
            }

            if (FindObjectOfType<AuthManager>() == null)
            {
                var authGO = new GameObject("AuthManager");
                authGO.AddComponent<AuthManager>();
                DontDestroyOnLoad(authGO);
                Debug.Log("[GameManager] AuthManager created");
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[GameManager] Scene loaded: {scene.name}");
        }

        public async void HandleLoginSuccess()
        {
            Debug.Log("[GameManager] Login successful, transitioning to lobby...");
            
            OnAuthSuccess?.Invoke();
            
            await SceneController.LoadSceneAsync(lobbyScene);
        }

        public async void HandleAuthFailure()
        {
            Debug.LogWarning("[GameManager] Authentication failed");
            
            OnAuthFailed?.Invoke();
        }

        public async Task NavigateToLogin()
        {
            AuthManager.Instance.Logout();
            await SceneController.LoadSceneAsync(loginScene);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (debugMode)
            {
                Debug.Log($"[GameManager] Application pause: {pauseStatus}");
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (debugMode)
            {
                Debug.Log($"[GameManager] Application focus: {hasFocus}");
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}