using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using ShipBattle.Network;
using System.Threading.Tasks;

namespace ShipBattle.Core
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.Log("[GameManager] Creating new GameManager instance");
                    var go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }
        
        [SerializeField] private AppStateMachine stateMachine;
        
        private MatchReadyEvent currentMatch;
        private MatchEndEvent matchResult;
        
        // Properties for backward compatibility/access
        public SocketClient SocketClient => SocketClient.Instance;
        
        // Properties for opponent info
        public string OpponentUsername => currentMatch?.opponentUsername ?? "";
        public int OpponentRating => currentMatch?.opponentRating ?? 0;
        
        private void Awake()
        {
            Debug.Log("[GameManager] Awake");
            
            if (instance != null && instance != this)
            {
                Debug.Log("[GameManager] Duplicate instance found - destroying");
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameManager] Instance set and marked as DontDestroyOnLoad");
        }
        
        public void Initialize()
        {
            Debug.Log("[GameManager] Initialize called");
            
            // Initialize state machine if needed
            if (stateMachine == null) 
            {
                stateMachine = new AppStateMachine();
                Debug.Log("[GameManager] State machine created");
            }
            
            Debug.Log("[GameManager] Initialization complete");
        }
        
        private void Start()
        {
            Debug.Log("[GameManager] Start");
        }
        
        public void SetCurrentMatch(MatchReadyEvent match)
        {
            Debug.Log($"[GameManager] Setting current match: Opponent={match?.opponentUsername}");
            currentMatch = match;
        }
        
        public MatchReadyEvent GetCurrentMatch()
        {
            return currentMatch;
        }
        
        public void SetMatchResult(MatchEndEvent result)
        {
            Debug.Log($"[GameManager] Setting match result: Winner={result?.winnerId}, RatingChange={result?.ratingChange}");
            matchResult = result;
        }
        
        public MatchEndEvent GetMatchResult()
        {
            return matchResult;
        }
        
        public void LoadLobby()
        {
            Debug.Log("[GameManager] Loading Lobby scene");
            
            // Clear current match data when returning to lobby
            currentMatch = null;
            matchResult = null;
            
            Debug.Log("[GameManager] Match data cleared");
            SceneManager.LoadScene("Lobby");
        }
        
        private void OnApplicationQuit()
        {
            Debug.Log("[GameManager] Application quitting");
        }
        
        private void OnDestroy()
        {
            Debug.Log("[GameManager] OnDestroy");
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
