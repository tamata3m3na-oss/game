using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShipBattle.Core
{
    /// <summary>
    /// Bootstrap class that initializes the game and manages the entry point.
    /// Enhanced with detailed logging.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        private static bool isInitialized = false;

        private void Awake()
        {
            Debug.Log("[GameBootstrap] Awake called");
            
            if (!isInitialized)
            {
                Debug.Log("[GameBootstrap] First initialization - setting up DontDestroyOnLoad");
                isInitialized = true;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else
            {
                Debug.Log("[GameBootstrap] Already initialized - destroying duplicate");
                Destroy(gameObject);
            }
        }

        private void InitializeGame()
        {
            Debug.Log("[GameBootstrap] ========================================");
            Debug.Log("[GameBootstrap] Initializing Game...");
            Debug.Log("[GameBootstrap] ========================================");
            
            // Set target frame rate
            Application.targetFrameRate = 60;
            Debug.Log("[GameBootstrap] Target frame rate set to 60");
            
            // Prevent screen sleep
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Debug.Log("[GameBootstrap] Screen sleep timeout set to NeverSleep");
            
            // Log platform info
            Debug.Log($"[GameBootstrap] Platform: {Application.platform}");
            Debug.Log($"[GameBootstrap] Unity Version: {Application.unityVersion}");
            Debug.Log($"[GameBootstrap] Persistent Data Path: {Application.persistentDataPath}");
            
            // Initialize core systems
            Debug.Log("[GameBootstrap] Initializing GameManager...");
            GameManager.Instance.Initialize();
            
            Debug.Log("[GameBootstrap] ========================================");
            Debug.Log("[GameBootstrap] Game initialized successfully!");
            Debug.Log("[GameBootstrap] ========================================");
        }
        
        private void OnApplicationQuit()
        {
            Debug.Log("[GameBootstrap] Application quitting...");
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            Debug.Log($"[GameBootstrap] Application pause status: {pauseStatus}");
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            Debug.Log($"[GameBootstrap] Application focus status: {hasFocus}");
        }
    }
}
