using System.Collections.Generic;
using UnityEngine;
using UI.Animations;

/// <summary>
/// ManagersSafetyCheck:
/// - Provides comprehensive safety checks for all singleton managers
/// - Logs detailed information about manager initialization status
/// - Helps identify null reference issues during development
/// </summary>
public static class ManagersSafetyCheck
{
    private static readonly List<string> checkedManagers = new List<string>();
    private static readonly List<string> failedChecks = new List<string>();

    /// <summary>
    /// Perform comprehensive safety check on all critical managers
    /// </summary>
    /// <param name="context">Context for logging (e.g., "Bootstrap", "SceneLoad")</param>
    /// <returns>True if all managers are properly initialized, false otherwise</returns>
    public static bool CheckAllManagers(string context = "Unknown")
    {
        checkedManagers.Clear();
        failedChecks.Clear();

        bool allManagersReady = true;

        // Check core managers
        allManagersReady &= CheckManager("ThreadSafeEventQueue", ThreadSafeEventQueue.Instance != null, context);
        allManagersReady &= CheckManager("NetworkEventManager", NetworkEventManager.GetInstance(false) != null, context);
        allManagersReady &= CheckManager("NetworkManager", NetworkManager.Instance != null, context);
        allManagersReady &= CheckManager("AuthManager", AuthManager.Instance != null, context);
        allManagersReady &= CheckManager("InputController", InputController.Instance != null, context);

        // Check game state managers
        allManagersReady &= CheckManager("GameStateRepository", GameStateRepository.GetInstance(false) != null, context);
        allManagersReady &= CheckManager("GameTickManager", GameTickManager.GetInstance(false) != null, context);
        allManagersReady &= CheckManager("SnapshotProcessor", SnapshotProcessor.GetInstance(false) != null, context);

        // Check UI managers (critical for this fix)
        allManagersReady &= CheckManager("AnimationController", AnimationController.Instance != null, context);
        allManagersReady &= CheckManager("ParticleController", ParticleController.Instance != null, context);
        allManagersReady &= CheckManager("TransitionManager", TransitionManager.Instance != null, context);

        // Check scene managers
        allManagersReady &= CheckManager("SceneInitializer", Object.FindObjectOfType<SceneInitializer>() != null, context);

        // Log summary
        LogSafetyCheckSummary(context, allManagersReady);

        return allManagersReady;
    }

    /// <summary>
    /// Check a specific manager and log results
    /// </summary>
    private static bool CheckManager(string managerName, bool isReady, string context)
    {
        checkedManagers.Add(managerName);

        if (!isReady)
        {
            failedChecks.Add(managerName);
            Debug.LogError($"[ManagersSafetyCheck] [{context}] Manager '{managerName}' is NULL - this will cause runtime errors!");
            return false;
        }

        Debug.Log($"[ManagersSafetyCheck] [{context}] Manager '{managerName}' is ready.");
        return true;
    }

    /// <summary>
    /// Log comprehensive summary of safety check
    /// </summary>
    private static void LogSafetyCheckSummary(string context, bool allManagersReady)
    {
        string summaryMessage;
        
        if (allManagersReady)
        {
            summaryMessage = $"[ManagersSafetyCheck] [{context}] ✅ All {checkedManagers.Count} managers are properly initialized.";
            Debug.Log(summaryMessage);
        }
        else
        {
            summaryMessage = $"[ManagersSafetyCheck] [{context}] ❌ {failedChecks.Count} out of {checkedManagers.Count} managers failed initialization check!";
            Debug.LogError(summaryMessage);
            
            // Log detailed failure information
            foreach (string failedManager in failedChecks)
            {
                Debug.LogError($"[ManagersSafetyCheck] [{context}] ❌ {failedManager} - NULL reference detected");
            }
            
            // Provide guidance for common issues
            if (failedChecks.Contains("ParticleController") || failedChecks.Contains("TransitionManager"))
            {
                Debug.LogError("[ManagersSafetyCheck] CRITICAL: ParticleController or TransitionManager is null. " +
                              "This typically indicates a bootstrap order issue. " +
                              "Check DefaultExecutionOrder attributes and ensure proper initialization sequence.");
            }
        }
    }

    /// <summary>
    /// Check if a specific manager is ready with detailed logging
    /// </summary>
    public static bool IsManagerReady<T>(string managerName, string context = "Runtime") where T : class
    {
        bool isReady = typeof(T) == typeof(ParticleController) ? ParticleController.Instance != null :
                      typeof(T) == typeof(TransitionManager) ? TransitionManager.Instance != null :
                      typeof(T) == typeof(AnimationController) ? AnimationController.Instance != null :
                      false;

        if (!isReady)
        {
            Debug.LogWarning($"[ManagersSafetyCheck] [{context}] Manager '{managerName}' is not ready for use.");
        }

        return isReady;
    }

    /// <summary>
    /// Safe method to get ParticleController instance with fallback handling
    /// </summary>
    public static ParticleController GetParticleControllerSafe(string context = "Runtime")
    {
        if (ParticleController.Instance != null)
        {
            return ParticleController.Instance;
        }
        
        Debug.LogWarning($"[ManagersSafetyCheck] [{context}] ParticleController.Instance is null. Returning null with graceful fallback.");
        return null;
    }

    /// <summary>
    /// Safe method to get TransitionManager instance with fallback handling
    /// </summary>
    public static TransitionManager GetTransitionManagerSafe(string context = "Runtime")
    {
        if (TransitionManager.Instance != null)
        {
            return TransitionManager.Instance;
        }
        
        Debug.LogWarning($"[ManagersSafetyCheck] [{context}] TransitionManager.Instance is null. Returning null with graceful fallback.");
        return null;
    }

    /// <summary>
    /// Safe method to get AnimationController instance with fallback handling
    /// </summary>
    public static AnimationController GetAnimationControllerSafe(string context = "Runtime")
    {
        if (AnimationController.Instance != null)
        {
            return AnimationController.Instance;
        }
        
        Debug.LogWarning($"[ManagersSafetyCheck] [{context}] AnimationController.Instance is null. Returning null with graceful fallback.");
        return null;
    }
}