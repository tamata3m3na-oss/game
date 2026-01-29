using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using ShipBattle.Network;
using ShipBattle.Network.Models;
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
            // Initialize state machine if needed
            if (stateMachine == null) stateMachine = new AppStateMachine();
            
            Debug.Log("[GameManager] Initialized");
        }
        
        private void Start()
        {
            // Logic from ticket: ensure we start at Splash if not already
            // This might cause loop if Splash loads GameManager which loads Splash.
            // Assuming this script is on a persistent object created in Splash.
        }
        
        public void SetCurrentMatch(MatchReadyEvent match)
        {
            currentMatch = match;
        }
        
        public MatchReadyEvent GetCurrentMatch()
        {
            return currentMatch;
        }
        
        public void SetMatchResult(MatchEndEvent result)
        {
            matchResult = result;
        }
        
        public MatchEndEvent GetMatchResult()
        {
            return matchResult;
        }
    }
}
