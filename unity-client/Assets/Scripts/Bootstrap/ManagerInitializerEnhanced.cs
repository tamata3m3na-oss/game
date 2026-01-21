using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Text;

namespace UI.BootstrapDiagnostics
{
    /// <summary>
    /// Enhanced ManagerInitializer with detailed diagnostics and recovery mechanisms
    /// Provides step-by-step logging and automatic recovery for failed initializations
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class ManagerInitializerEnhanced : MonoBehaviour
    {
        private static ManagerInitializerEnhanced instance;
        public static ManagerInitializerEnhanced Instance => instance;

        public static bool IsReady => instance != null && instance.isReady;

        [Header("Enhanced Bootstrap Settings")]
        [SerializeField] private float initializationTimeoutSeconds = 15f;
        [SerializeField] private float stepTimeoutSeconds = 3f;
        [SerializeField] private bool enableAutoRecovery = true;
        [SerializeField] private bool enableDetailedLogging = true;

        private bool isReady;
        private bool isInitializing;
        private Stopwatch stopwatch;
        private StringBuilder diagnosticLog;
        private Dictionary<string, bool> initializationSteps;
        private List<string> failedSteps;
        private List<string> recoveryAttempts;

        // Events for UI and testing
        public System.Action<string> OnBootstrapStepCompleted;
        public System.Action<string> OnBootstrapFailed;
        public System.Action OnBootstrapCompleted;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeDiagnosticSystem();
        }

        private void Start()
        {
            StartCoroutine(EnhancedInitializeManagers());
        }

        private void InitializeDiagnosticSystem()
        {
            stopwatch = new Stopwatch();
            diagnosticLog = new StringBuilder();
            initializationSteps = new Dictionary<string, bool>();
            failedSteps = new List<string>();
            recoveryAttempts = new List<string>();

            LogDiagnostic("[ManagerInitializerEnhanced] Diagnostic system initialized");
        }

        private IEnumerator EnhancedInitializeManagers()
        {
            if (isInitializing)
            {
                LogDiagnostic("[ManagerInitializerEnhanced] Already initializing, skipping duplicate start");
                yield break;
            }

            isInitializing = true;
            stopwatch.Start();

            LogDiagnostic("[ManagerInitializerEnhanced] Starting enhanced manager initialization...");
            LogDiagnostic($"[ManagerInitializerEnhanced] Timeout settings - Total: {initializationTimeoutSeconds}s, Per step: {stepTimeoutSeconds}s");

            // Step 1: Ensure BootstrapRunner exists
            yield return EnsureBootstrapRunnerExistsEnhanced();

            // Step 2: Initialize core managers with detailed tracking
            yield return InitializeCoreManagersEnhanced();

            // Step 3: Initialize UI managers (critical for our fix)
            yield return InitializeUIManagersEnhanced();

            // Step 4: Initialize game state managers
            yield return InitializeGameStateManagersEnhanced();

            // Step 5: Final verification
            yield return PerformFinalVerification();

            stopwatch.Stop();
            isInitializing = false;

            if (isReady)
            {
                LogDiagnostic($"[ManagerInitializerEnhanced] ✅ Bootstrap completed successfully in {stopwatch.ElapsedMilliseconds}ms");
                OnBootstrapCompleted?.Invoke();
            }
            else
            {
                string errorMsg = $"[ManagerInitializerEnhanced] ❌ Bootstrap failed after {stopwatch.ElapsedMilliseconds}ms";
                LogDiagnostic(errorMsg);
                OnBootstrapFailed?.Invoke(errorMsg);
            }
        }

        private IEnumerator EnsureBootstrapRunnerExistsEnhanced()
        {
            string stepName = "EnsureBootstrapRunner";
            LogDiagnostic($"[ManagerInitializerEnhanced] Step 1: {stepName}");

            float stepStartTime = Time.time;

            BootstrapRunner existingRunner = FindObjectOfType<BootstrapRunner>();
            if (existingRunner != null)
            {
                LogDiagnostic($"[ManagerInitializerEnhanced] ✅ {stepName} - BootstrapRunner already exists");
                MarkStepComplete(stepName);
                yield break;
            }

            // Try to find bootstrap game object
            GameObject bootstrapGo = GameObject.Find("_Bootstrap");
            if (bootstrapGo == null)
            {
                bootstrapGo = new GameObject("_Bootstrap");
                LogDiagnostic($"[ManagerInitializerEnhanced] 📦 {stepName} - Created new Bootstrap GameObject");
            }
            else
            {
                LogDiagnostic($"[ManagerInitializerEnhanced] 🔍 {stepName} - Found existing Bootstrap GameObject");
            }

            Object.DontDestroyOnLoad(bootstrapGo);

            // Add BootstrapRunner component
            BootstrapRunner runner = bootstrapGo.GetComponent<BootstrapRunner>();
            if (runner == null)
            {
                runner = bootstrapGo.AddComponent<BootstrapRunner>();
                LogDiagnostic($"[ManagerInitializerEnhanced] ⚙️ {stepName} - Added BootstrapRunner component");
            }

            // Wait for component to initialize
            yield return WaitForComponentInitialized(runner, stepName);

            float stepDuration = Time.time - stepStartTime;
            LogDiagnostic($"[ManagerInitializerEnhanced] ⏱️ {stepName} completed in {stepDuration:F2}s");

            MarkStepComplete(stepName);
        }

        private IEnumerator InitializeCoreManagersEnhanced()
        {
            string stepName = "InitializeCoreManagers";
            LogDiagnostic($"[ManagerInitializerEnhanced] Step 2: {stepName}");

            yield return InitializeManagerWithRetry<ThreadSafeEventQueue>(stepName, "ThreadSafeEventQueue");
            yield return InitializeManagerWithRetry<NetworkEventManager>(stepName, "NetworkEventManager");
            yield return InitializeManagerWithRetry<NetworkManager>(stepName, "NetworkManager");

            MarkStepComplete(stepName);
        }

        private IEnumerator InitializeUIManagersEnhanced()
        {
            string stepName = "InitializeUIManagers";
            LogDiagnostic($"[ManagerInitializerEnhanced] Step 3: {stepName}");

            yield return InitializeManagerWithRetry<ParticleController>(stepName, "ParticleController");
            yield return InitializeManagerWithRetry<AnimationController>(stepName, "AnimationController");
            yield return InitializeManagerWithRetry<TransitionManager>(stepName, "TransitionManager");

            MarkStepComplete(stepName);
        }

        private IEnumerator InitializeGameStateManagersEnhanced()
        {
            string stepName = "InitializeGameStateManagers";
            LogDiagnostic($"[ManagerInitializerEnhanced] Step 4: {stepName}");

            yield return InitializeManagerWithRetry<AuthManager>(stepName, "AuthManager");
            yield return InitializeManagerWithRetry<InputController>(stepName, "InputController");
            yield return InitializeManagerWithRetry<GameStateRepository>(stepName, "GameStateRepository");
            yield return InitializeManagerWithRetry<GameTickManager>(stepName, "GameTickManager");
            yield return InitializeManagerWithRetry<SnapshotProcessor>(stepName, "SnapshotProcessor");

            MarkStepComplete(stepName);
        }

        private IEnumerator InitializeManagerWithRetry<T>(string parentStep, string managerName) where T : Component
        {
            LogDiagnostic($"[ManagerInitializerEnhanced] 🔧 {parentStep} - Initializing {managerName}");

            int maxRetries = enableAutoRecovery ? 3 : 1;
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                float attemptStartTime = Time.time;

                // Check if manager already exists
                T existingManager = FindObjectOfType<T>();
                if (existingManager != null)
                {
                    LogDiagnostic($"[ManagerInitializerEnhanced] ✅ {managerName} - Already initialized (attempt {attempt})");
                    yield return WaitForManagerAwakeComplete(existingManager);
                    
                    float attemptDuration = Time.time - attemptStartTime;
                    LogDiagnostic($"[ManagerInitializerEnhanced] ⏱️ {managerName} verified in {attemptDuration:F2}s");
                    yield break;
                }

                // Create manager if needed
                GameObject bootstrapGo = GameObject.Find("_Bootstrap");
                if (bootstrapGo != null)
                {
                    T manager = bootstrapGo.GetComponent<T>();
                    if (manager == null)
                    {
                        manager = bootstrapGo.AddComponent<T>();
                        LogDiagnostic($"[ManagerInitializerEnhanced] 📦 {managerName} - Added component to Bootstrap");
                    }

                    // Wait for initialization
                    yield return WaitForManagerAwakeComplete(manager);

                    // Verify manager is working
                    yield return VerifyManagerFunctioning<T>(manager, managerName);
                    
                    float attemptDuration = Time.time - attemptStartTime;
                    LogDiagnostic($"[ManagerInitializerEnhanced] ⏱️ {managerName} attempt {attempt} completed in {attemptDuration:F2}s");
                    
                    // If auto-recovery is enabled and we succeeded, mark step as complete
                    if (enableAutoRecovery && attempt > 1)
                    {
                        recoveryAttempts.Add($"{managerName} (attempt {attempt})");
                    }
                    
                    yield break;
                }
                else
                {
                    LogDiagnostic($"[ManagerInitializerEnhanced] ⚠️ {managerName} - Bootstrap GameObject not found (attempt {attempt})");
                }

                if (attempt < maxRetries)
                {
                    float retryDelay = attempt * 0.5f; // Progressive delay
                    LogDiagnostic($"[ManagerInitializerEnhanced] 🔄 {managerName} - Retrying in {retryDelay}s...");
                    yield return new WaitForSeconds(retryDelay);
                }
            }

            // If we reach here, all attempts failed
            LogDiagnostic($"[ManagerInitializerEnhanced] ❌ {managerName} - Failed to initialize after {maxRetries} attempts");
            failedSteps.Add(managerName);
        }

        private IEnumerator WaitForComponentInitialized(Component component, string componentName)
        {
            float timeout = stepTimeoutSeconds;
            float startTime = Time.time;

            // Wait for the component to be fully initialized
            while (Time.time - startTime < timeout)
            {
                // Check if component is properly initialized
                if (component != null && component.gameObject != null)
                {
                    yield return null; // Give one more frame for Awake to complete
                    yield break;
                }
                yield return null;
            }

            LogDiagnostic($"[ManagerInitializerEnhanced] ⚠️ {componentName} - Initialization timeout after {timeout}s");
        }

        private IEnumerator WaitForManagerAwakeComplete<T>(T manager) where T : Component
        {
            // Give the manager's Awake method time to complete
            yield return null;
            yield return null; // Two frames should be enough for Awake to complete
        }

        private IEnumerator VerifyManagerFunctioning<T>(T manager, string managerName) where T : Component
        {
            if (manager == null)
            {
                LogDiagnostic($"[ManagerInitializerEnhanced] ❌ {managerName} - Manager is null after initialization");
                yield break;
            }

            // Check if manager has proper instance set (for singleton managers)
            if (manager is ParticleController pc && pc.Instance != manager)
            {
                LogDiagnostic($"[ManagerInitializerEnhanced] ⚠️ {managerName} - Instance not set correctly");
            }
            else if (manager is TransitionManager tm && tm.Instance != manager)
            {
                LogDiagnostic($"[ManagerInitializerEnhanced] ⚠️ {managerName} - Instance not set correctly");
            }
            else if (manager is AnimationController ac && ac.Instance != manager)
            {
                LogDiagnostic($"[ManagerInitializerEnhanced] ⚠️ {managerName} - Instance not set correctly");
            }
            else
            {
                LogDiagnostic($"[ManagerInitializerEnhanced] ✅ {managerName} - Instance set correctly");
            }

            yield return null;
        }

        private IEnumerator PerformFinalVerification()
        {
            string stepName = "FinalVerification";
            LogDiagnostic($"[ManagerInitializerEnhanced] Step 5: {stepName}");

            // Use the comprehensive safety check
            bool allManagersReady = ManagersSafetyCheck.CheckAllManagers("EnhancedBootstrap");

            if (allManagersReady && failedSteps.Count == 0)
            {
                isReady = true;
                LogDiagnostic($"[ManagerInitializerEnhanced] ✅ All managers verified successfully");
            }
            else
            {
                LogDiagnostic($"[ManagerInitializerEnhanced] ❌ Final verification failed:");
                foreach (string failedStep in failedSteps)
                {
                    LogDiagnostic($"[ManagerInitializerEnhanced]   - {failedStep}");
                }
            }

            MarkStepComplete(stepName);
        }

        private void MarkStepComplete(string stepName)
        {
            if (!initializationSteps.ContainsKey(stepName))
            {
                initializationSteps.Add(stepName, true);
                OnBootstrapStepCompleted?.Invoke(stepName);
            }
        }

        private void LogDiagnostic(string message)
        {
            if (enableDetailedLogging)
            {
                Debug.Log(message);
                diagnosticLog.AppendLine(message);
            }
        }

        /// <summary>
        /// Get detailed diagnostic information
        /// </summary>
        public string GetDiagnosticReport()
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("=== ENHANCED BOOTSTRAP DIAGNOSTIC REPORT ===");
            report.AppendLine($"Timestamp: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Total Duration: {stopwatch.ElapsedMilliseconds}ms");
            report.AppendLine($"Is Ready: {isReady}");
            report.AppendLine($"Failed Steps: {failedSteps.Count}");
            report.AppendLine($"Recovery Attempts: {recoveryAttempts.Count}");
            
            report.AppendLine("\n=== INITIALIZATION STEPS ===");
            foreach (var step in initializationSteps)
            {
                report.AppendLine($"✅ {step.Key}");
            }
            
            if (failedSteps.Count > 0)
            {
                report.AppendLine("\n=== FAILED STEPS ===");
                foreach (string failedStep in failedSteps)
                {
                    report.AppendLine($"❌ {failedStep}");
                }
            }
            
            if (recoveryAttempts.Count > 0)
            {
                report.AppendLine("\n=== RECOVERY ATTEMPTS ===");
                foreach (string recovery in recoveryAttempts)
                {
                    report.AppendLine($"🔄 {recovery}");
                }
            }
            
            report.AppendLine("\n=== DETAILED LOG ===");
            report.AppendLine(diagnosticLog.ToString());
            
            return report.ToString();
        }

        /// <summary>
        /// Force re-initialization (for testing)
        /// </summary>
        public void ForceReinitialize()
        {
            if (isInitializing) return;

            LogDiagnostic("[ManagerInitializerEnhanced] Force re-initialization requested");
            isReady = false;
            StartCoroutine(EnhancedInitializeManagers());
        }

        /// <summary>
        /// Notify completion (called by BootstrapRunner)
        /// </summary>
        public void NotifyBootstrapCompleted()
        {
            if (!isReady)
            {
                isReady = true;
                LogDiagnostic("[ManagerInitializerEnhanced] Bootstrap completion notified");
                OnBootstrapCompleted?.Invoke();
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// Get current bootstrap status
        /// </summary>
        public BootstrapStatus GetCurrentStatus()
        {
            return new BootstrapStatus
            {
                IsReady = isReady,
                IsInitializing = isInitializing,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                FailedStepsCount = failedSteps.Count,
                CompletedStepsCount = initializationSteps.Count,
                RecoveryAttemptsCount = recoveryAttempts.Count,
                HasErrors = failedSteps.Count > 0
            };
        }
    }

    /// <summary>
    /// Bootstrap status information for external access
    /// </summary>
    [System.Serializable]
    public struct BootstrapStatus
    {
        public bool IsReady;
        public bool IsInitializing;
        public long ElapsedMilliseconds;
        public int FailedStepsCount;
        public int CompletedStepsCount;
        public int RecoveryAttemptsCount;
        public bool HasErrors;
    }
}