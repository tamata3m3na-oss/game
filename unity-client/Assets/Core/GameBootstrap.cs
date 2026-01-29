using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShipBattle.Core
{
    /// <summary>
    /// Bootstrap class that initializes the game and manages the entry point.
    /// Do not modify - this is Phase 1 completed code.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        private static bool isInitialized = false;

        private void Awake()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeGame()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            
            // Initialize core systems
            GameManager.Instance.Initialize();
            
            Debug.Log("[GameBootstrap] Game initialized successfully");
        }
    }
}
