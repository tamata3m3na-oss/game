using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ShipBattle.Network;

namespace ShipBattle.Core
{
    /// <summary>
    /// Central game manager handling scene transitions and game flow.
    /// Integrates with Network layer and manages match lifecycle.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private AppStateMachine stateMachine;
        private SocketClient socketClient;

        public AppStateMachine StateMachine => stateMachine;
        public SocketClient SocketClient => socketClient;

        // Match data
        private int currentMatchId;
        private string opponentUsername;
        private int opponentRating;
        private bool isMatchActive;

        public int CurrentMatchId => currentMatchId;
        public string OpponentUsername => opponentUsername;
        public int OpponentRating => opponentRating;
        public bool IsMatchActive => isMatchActive;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Initialize()
        {
            stateMachine = new AppStateMachine();
            socketClient = new SocketClient();
            
            // Subscribe to network events
            socketClient.OnMatchReady += HandleMatchReady;
            socketClient.OnMatchEnd += HandleMatchEnd;
            
            Debug.Log("[GameManager] Initialized successfully");
        }

        // Called from LoginController after successful authentication
        public async Task HandleLoginSuccess()
        {
            Debug.Log("[GameManager] Login successful, transitioning to Lobby");
            stateMachine.ChangeState(AppState.Lobby);
            await LoadSceneAsync("Lobby");
        }

        // Called from LoginController on authentication failure
        public async Task HandleAuthFailure()
        {
            Debug.LogError("[GameManager] Authentication failed");
            stateMachine.ChangeState(AppState.Login);
            await LoadSceneAsync("Login");
        }

        // Match lifecycle handlers
        private void HandleMatchReady(MatchReadyData data)
        {
            Debug.Log($"[GameManager] Match ready - ID: {data.matchId}, Opponent: {data.opponent.username}");
            
            currentMatchId = data.matchId;
            opponentUsername = data.opponent.username;
            opponentRating = data.opponent.rating;
            isMatchActive = true;
            
            stateMachine.ChangeState(AppState.InGame);
            LoadSceneAsync("Game");
        }

        private void HandleMatchEnd(MatchEndData data)
        {
            Debug.Log($"[GameManager] Match ended - Winner ID: {data.winnerId}");
            
            isMatchActive = false;
            
            stateMachine.ChangeState(AppState.Result);
            LoadSceneAsync("Result");
        }

        // Scene management
        private async Task LoadSceneAsync(string sceneName)
        {
            try
            {
                AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
                operation.allowSceneActivation = false;

                while (operation.progress < 0.9f)
                {
                    await Task.Delay(10);
                }

                operation.allowSceneActivation = true;
                
                while (!operation.isDone)
                {
                    await Task.Delay(10);
                }

                Debug.Log($"[GameManager] Scene loaded: {sceneName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameManager] Failed to load scene {sceneName}: {e.Message}");
            }
        }

        // Public API for scene transitions
        public void LoadLobby()
        {
            stateMachine.ChangeState(AppState.Lobby);
            SceneManager.LoadScene("Lobby");
        }

        public void LoadLogin()
        {
            stateMachine.ChangeState(AppState.Login);
            SceneManager.LoadScene("Login");
        }

        private void OnDestroy()
        {
            if (socketClient != null)
            {
                socketClient.OnMatchReady -= HandleMatchReady;
                socketClient.OnMatchEnd -= HandleMatchEnd;
            }
        }
    }
}
