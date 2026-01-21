#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define BOOTSTRAP_DEBUG
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UI.BootstrapDiagnostics
{
    /// <summary>
    /// Enhanced Bootstrap System with comprehensive error handling and diagnostics
    /// Replaces the original BootstrapRunner with improved initialization logic
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class BootstrapRunnerEnhanced : MonoBehaviour
    {
        private static BootstrapRunnerEnhanced instance;
        public static BootstrapRunnerEnhanced Instance => instance;

        [Header("Enhanced Bootstrap Settings")]
        [SerializeField] private bool enableDetailedLogging = true;
        [SerializeField] private bool enablePerformanceMonitoring = true;
        [SerializeField] private bool enableAutoRecovery = true;
        [SerializeField] private int maxRetriesPerManager = 3;

        private Stopwatch bootstrapStopwatch;
        private Dictionary<string, float> managerInitTimes;
        private List<string> initializationLog;
        private bool isInitialized = false;

        // Events for UI and testing
        public System.Action<string> OnManagerInitialized;
        public System.Action<string> OnManagerFailed;
        public System.Action OnBootstrapCompleted;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning("[BootstrapRunnerEnhanced] Duplicate instance detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeDiagnostics();
        }

        private void Start()
        {
            StartCoroutine(EnhancedBootstrapSequence());
        }

        private void InitializeDiagnostics()
        {
            bootstrapStopwatch = new Stopwatch();
            managerInitTimes = new Dictionary<string, float>();
            initializationLog = new List<string>();
            
            LogMessage("[BootstrapRunnerEnhanced] Enhanced bootstrap diagnostics initialized");
        }

        private IEnumerator EnhancedBootstrapSequence()
        {
            bootstrapStopwatch.Start();
            LogMessage("[BootstrapRunnerEnhanced] 🚀 Starting enhanced bootstrap sequence...");

            // Ensure ManagerInitializerEnhanced exists first
            yield return EnsureManagerInitializerExists();

            // Initialize core managers with detailed tracking
            yield return InitializeCoreManagers();

            // Initialize UI managers (critical for this fix)
            yield return InitializeUIManagers();

            // Initialize game state managers
            yield return InitializeGameStateManagers();

            // Initialize auth and input managers
            yield return InitializeAuthAndInputManagers();

            // Perform comprehensive verification
            yield return PerformComprehensiveVerification();

            bootstrapStopwatch.Stop();
            
            if (!isInitialized)
            {
                LogMessage($"[BootstrapRunnerEnhanced] ❌ Bootstrap failed after {bootstrapStopwatch.ElapsedMilliseconds}ms");
                yield break;
            }

            LogMessage($"[BootstrapRunnerEnhanced] ✅ Enhanced bootstrap completed successfully in {bootstrapStopwatch.ElapsedMilliseconds}ms");
            OnBootstrapCompleted?.Invoke();

            // Notify ManagerInitializerEnhanced
            var managerInitializer = FindObjectOfType<ManagerInitializerEnhanced>();
            if (managerInitializer != null)
            {
                managerInitializer.NotifyBootstrapCompleted();
            }
        }

        private IEnumerator EnsureManagerInitializerExists()
        {
            LogMessage("[BootstrapRunnerEnhanced] Step 1: Ensuring ManagerInitializerEnhanced exists...");
            
            ManagerInitializerEnhanced existingInitializer = FindObjectOfType<ManagerInitializerEnhanced>();
            if (existingInitializer != null)
            {
                LogMessage("[BootstrapRunnerEnhanced] ✅ ManagerInitializerEnhanced already exists");
                yield break;
            }

            GameObject initializerGo = new GameObject("_ManagerInitializerEnhanced");
            DontDestroyOnLoad(initializerGo);
            initializerGo.AddComponent<ManagerInitializerEnhanced>();
            
            LogMessage("[BootstrapRunnerEnhanced] ✅ Created ManagerInitializerEnhanced");
            yield return null;
        }

        private IEnumerator InitializeCoreManagers()
        {
            LogMessage("[BootstrapRunnerEnhanced] Step 2: Initializing core managers...");
            
            yield return InitializeManagerWithEnhancedLogic<ThreadSafeEventQueue>("ThreadSafeEventQueue");
            yield return InitializeManagerWithEnhancedLogic<NetworkEventManager>("NetworkEventManager");
            yield return InitializeManagerWithEnhancedLogic<NetworkManager>("NetworkManager");
        }

        private IEnumerator InitializeUIManagers()
        {
            LogMessage("[BootstrapRunnerEnhanced] Step 3: Initializing UI managers (critical for fix)...");
            
            // Initialize ParticleController first due to dependency chain
            yield return InitializeManagerWithEnhancedLogic<ParticleController>("ParticleController");
            
            // Initialize AnimationController 
            yield return InitializeManagerWithEnhancedLogic<AnimationController>("AnimationController");
            
            // Initialize TransitionManager
            yield return InitializeManagerWithEnhancedLogic<TransitionManager>("TransitionManager");
        }

        private IEnumerator InitializeGameStateManagers()
        {
            LogMessage("[BootstrapRunnerEnhanced] Step 4: Initializing game state managers...");
            
            yield return InitializeManagerWithEnhancedLogic<GameStateRepository>("GameStateRepository");
            yield return InitializeManagerWithEnhancedLogic<GameTickManager>("GameTickManager");
            yield return InitializeManagerWithEnhancedLogic<SnapshotProcessor>("SnapshotProcessor");
        }

        private IEnumerator InitializeAuthAndInputManagers()
        {
            LogMessage("[BootstrapRunnerEnhanced] Step 5: Initializing auth and input managers...");
            
            yield return InitializeManagerWithEnhancedLogic<AuthManager>("AuthManager");
            yield return InitializeManagerWithEnhancedLogic<InputController>("InputController");
            
            // Initialize SceneInitializer
            yield return InitializeManagerWithEnhancedLogic<SceneInitializer>("SceneInitializer");
        }

        private IEnumerator InitializeManagerWithEnhancedLogic<T>(string managerName) where T : Component
        {
            float startTime = Time.time;
            LogMessage($"[BootstrapRunnerEnhanced] 🔧 Initializing {managerName}...");

            int attempts = 0;
            bool success = false;

            while (attempts < maxRetriesPerManager && !success)
            {
                attempts++;
                
                try
                {
                    // Check if manager already exists
                    T existingManager = FindObjectOfType<T>();
                    if (existingManager != null)
                    {
                        LogMessage($"[BootstrapRunnerEnhanced] ✅ {managerName} already exists (attempt {attempts})");
                        success = true;
                        break;
                    }

                    // Ensure Bootstrap object exists
                    GameObject bootstrapGo = GetOrCreateBootstrapObject();
                    
                    // Add component
                    T manager = bootstrapGo.GetComponent<T>();
                    if (manager == null)
                    {
                        manager = bootstrapGo.AddComponent<T>();
                        LogMessage($"[BootstrapRunnerEnhanced] 📦 Added {managerName} component to Bootstrap");
                    }

                    // Wait for Awake to complete
                    yield return WaitForManagerAwake(manager);

                    // Verify manager is properly initialized
                    if (IsManagerProperlyInitialized<T>(manager, managerName))
                    {
                        float initTime = Time.time - startTime;
                        managerInitTimes[managerName] = initTime;
                        LogMessage($"[BootstrapRunnerEnhanced] ✅ {managerName} initialized successfully in {initTime:F3}s (attempt {attempts})");
                        success = true;
                        OnManagerInitialized?.Invoke(managerName);
                        
                        if (enableAutoRecovery && attempts > 1)
                        {
                            LogMessage($"[BootstrapRunnerEnhanced] 🔄 {managerName} required recovery (attempt {attempts})");
                        }
                        break;
                    }
                    else
                    {
                        LogMessage($"[BootstrapRunnerEnhanced] ⚠️ {managerName} verification failed (attempt {attempts})");
                    }
                }
                catch (System.Exception ex)
                {
                    LogMessage($"[BootstrapRunnerEnhanced] ❌ Exception initializing {managerName} (attempt {attempts}): {ex.Message}");
                }

                if (attempts < maxRetriesPerManager)
                {
                    float retryDelay = attempts * 0.5f; // Progressive delay
                    LogMessage($"[BootstrapRunnerEnhanced] 🔄 Retrying {managerName} in {retryDelay}s...");
                    yield return new WaitForSeconds(retryDelay);
                }
            }

            if (!success)
            {
                LogMessage($"[BootstrapRunnerEnhanced] ❌ {managerName} failed to initialize after {maxRetriesPerManager} attempts");
                OnManagerFailed?.Invoke(managerName);
            }

            yield return null;
        }

        private IEnumerator WaitForManagerAwake<T>(T manager) where T : Component
        {
            if (manager == null) yield break;

            // Give enough time for Awake method to complete
            yield return null; // Frame 1
            yield return null; // Frame 2
            yield return null; // Frame 3 (extra safety)
        }

        private bool IsManagerProperlyInitialized<T>(T manager, string managerName) where T : Component
        {
            if (manager == null) return false;

            // Check if manager has proper instance set (for singleton managers)
            if (manager is ParticleController pc && pc.Instance != manager)
            {
                LogMessage($"[BootstrapRunnerEnhanced] ⚠️ {managerName} - Instance not set correctly");
                return false;
            }
            else if (manager is TransitionManager tm && tm.Instance != manager)
            {
                LogMessage($"[BootstrapRunnerEnhanced] ⚠️ {managerName} - Instance not set correctly");
                return false;
            }
            else if (manager is AnimationController ac && ac.Instance != manager)
            {
                LogMessage($"[BootstrapRunnerEnhanced] ⚠️ {managerName} - Instance not set correctly");
                return false;
            }

            return true;
        }

        private GameObject GetOrCreateBootstrapObject()
        {
            GameObject bootstrapGo = GameObject.Find("_Bootstrap");
            if (bootstrapGo == null)
            {
                bootstrapGo = new GameObject("_Bootstrap");
                DontDestroyOnLoad(bootstrapGo);
                LogMessage("[BootstrapRunnerEnhanced] 📦 Created new Bootstrap GameObject");
            }
            return bootstrapGo;
        }

        private IEnumerator PerformComprehensiveVerification()
        {
            LogMessage("[BootstrapRunnerEnhanced] Step 6: Performing comprehensive verification...");
            
            // Use the enhanced safety check
            bool allManagersReady = ManagersSafetyCheck.CheckAllManagers("EnhancedBootstrap");
            
            if (allManagersReady)
            {
                LogMessage("[BootstrapRunnerEnhanced] ✅ All managers verified successfully");
                isInitialized = true;
            }
            else
            {
                LogMessage("[BootstrapRunnerEnhanced] ❌ Comprehensive verification failed");
            }

            yield return null;
        }

        private void LogMessage(string message)
        {
#if BOOTSTRAP_DEBUG
            if (enableDetailedLogging)
            {
                Debug.Log(message);
                initializationLog.Add(message);
            }
#endif
        }

        /// <summary>
        /// Get detailed bootstrap performance report
        /// </summary>
        public string GetBootstrapReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== ENHANCED BOOTSTRAP REPORT ===");
            report.AppendLine($"Timestamp: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Total Duration: {bootstrapStopwatch.ElapsedMilliseconds}ms");
            report.AppendLine($"Is Initialized: {isInitialized}");
            report.AppendLine($"Managers Initialized: {managerInitTimes.Count}");
            
            report.AppendLine("\n=== MANAGER INITIALIZATION TIMES ===");
            foreach (var kvp in managerInitTimes.OrderBy(x => x.Value))
            {
                report.AppendLine($"{kvp.Key}: {kvp.Value:F3}s");
            }
            
            if (initializationLog.Count > 0)
            {
                report.AppendLine("\n=== INITIALIZATION LOG ===");
                foreach (string log in initializationLog.Take(50)) // Limit to last 50 entries
                {
                    report.AppendLine(log);
                }
            }
            
            return report.ToString();
        }

        /// <summary>
        /// Force re-bootstrap (for testing and recovery)
        /// </summary>
        public void ForceRebootstrap()
        {
            LogMessage("[BootstrapRunnerEnhanced] Force re-bootstrap requested");
            isInitialized = false;
            StartCoroutine(EnhancedBootstrapSequence());
        }

        /// <summary>
        /// Get current bootstrap status
        /// </summary>
        public BootstrapStatus GetCurrentStatus()
        {
            return new BootstrapStatus
            {
                IsInitialized = isInitialized,
                ElapsedMilliseconds = bootstrapStopwatch.ElapsedMilliseconds,
                ManagersCount = managerInitTimes.Count,
                LogEntriesCount = initializationLog.Count
            };
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }

    /// <summary>
    /// Enhanced status structure for external access
    /// </summary>
    [System.Serializable]
    public struct BootstrapStatus
    {
        public bool IsInitialized;
        public long ElapsedMilliseconds;
        public int ManagersCount;
        public int LogEntriesCount;
    }
}