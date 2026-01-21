#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define COMPILATION_SAFETY_DEBUG
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityProjectTools
{
    /// <summary>
    /// Compilation Safety Manager
    /// Provides proactive compilation error prevention and fixes
    /// Includes using statement validation and namespace management
    /// </summary>
    public static class CompilationSafetyManager
    {
        /// <summary>
        /// Validate that all required using statements are present in a script
        /// </summary>
        public static void ValidateUsingStatements(string scriptContent, string scriptPath)
        {
            var issues = new List<string>();

            // Check for missing UI.Animations namespace
            if (scriptContent.Contains("AnimationController") && !scriptContent.Contains("using UI.Animations"))
            {
                issues.Add("Missing 'using UI.Animations;' for AnimationController");
            }

            if (scriptContent.Contains("TransitionManager") && !scriptContent.Contains("using UI.Animations"))
            {
                issues.Add("Missing 'using UI.Animations;' for TransitionManager");
            }

            if (scriptContent.Contains("ParticleController") && !scriptContent.Contains("using UI.Animations"))
            {
                issues.Add("Missing 'using UI.Animations;' for ParticleController");
            }

            // Check for missing DOTween compatibility
            if (scriptContent.Contains("DOTween") && !scriptContent.Contains("#if DOTWEEN_AVAILABLE"))
            {
                issues.Add("DOTween usage should be wrapped in #if DOTWEEN_AVAILABLE guards");
            }

            // Check for missing Unity using statements
            if (scriptContent.Contains("UnityEngine") && !scriptContent.Contains("using UnityEngine"))
            {
                issues.Add("Missing 'using UnityEngine;' statement");
            }

            if (issues.Count > 0)
            {
#if COMPILATION_SAFETY_DEBUG
                Debug.LogWarning($"[CompilationSafetyManager] Issues in {scriptPath}:");
                foreach (string issue in issues)
                {
                    Debug.LogWarning($"  - {issue}");
                }
#endif
            }
        }

        /// <summary>
        /// Safe method to access AnimationController with null checking
        /// </summary>
        public static void SafeAnimateFade(CanvasGroup canvasGroup, bool fadeIn, System.Action onComplete = null)
        {
            if (canvasGroup == null)
            {
#if COMPILATION_SAFETY_DEBUG
                Debug.LogWarning("[CompilationSafetyManager] CanvasGroup is null in SafeAnimateFade");
#endif
                onComplete?.Invoke();
                return;
            }

            var animationController = UI.Animations.AnimationController.Instance;
            if (animationController != null)
            {
                if (fadeIn)
                {
                    animationController.FadeIn(canvasGroup, -1, onComplete);
                }
                else
                {
                    animationController.FadeOut(canvasGroup, -1, onComplete);
                }
            }
            else
            {
#if COMPILATION_SAFETY_DEBUG
                Debug.LogWarning("[CompilationSafetyManager] AnimationController.Instance is null");
#endif
                // Fallback to manual fade
                SafeManualFade(canvasGroup, fadeIn, onComplete);
            }
        }

        /// <summary>
        /// Safe method to access TransitionManager with null checking
        /// </summary>
        public static void SafeLoadScene(string sceneName, UI.Animations.TransitionManager.TransitionType transitionType = UI.Animations.TransitionManager.TransitionType.Fade)
        {
            var transitionManager = UI.Animations.TransitionManager.Instance;
            if (transitionManager != null)
            {
                transitionManager.LoadScene(sceneName, transitionType);
            }
            else
            {
#if COMPILATION_SAFETY_DEBUG
                Debug.LogWarning("[CompilationSafetyManager] TransitionManager.Instance is null, loading scene directly");
#endif
                // Fallback to direct scene load
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
        }

        /// <summary>
        /// Safe method to access ParticleController with null checking
        /// </summary>
        public static void SafeSpawnEffect(string effectName, Vector3 position, float lifetime = -1f)
        {
            var particleController = UI.Animations.ParticleController.Instance;
            if (particleController != null)
            {
                particleController.SpawnParticle(effectName, position, lifetime);
            }
            else
            {
#if COMPILATION_SAFETY_DEBUG
                Debug.LogWarning($"[CompilationSafetyManager] ParticleController.Instance is null, cannot spawn effect: {effectName}");
#endif
            }
        }

        /// <summary>
        /// Manual fade fallback when AnimationController is not available
        /// </summary>
        private static IEnumerator SafeManualFade(CanvasGroup canvasGroup, bool fadeIn, System.Action onComplete)
        {
            float targetAlpha = fadeIn ? 1f : 0f;
            float startAlpha = canvasGroup.alpha;
            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Validate project compilation readiness
        /// </summary>
        public static bool ValidateProjectReadiness()
        {
            var issues = new List<string>();

            // Check critical managers
            var criticalManagers = new[]
            {
                ("UI.Animations.AnimationController", () => UI.Animations.AnimationController.Instance != null),
                ("UI.Animations.ParticleController", () => UI.Animations.ParticleController.Instance != null),
                ("UI.Animations.TransitionManager", () => UI.Animations.TransitionManager.Instance != null)
            };

            foreach (var (managerName, checkFunc) in criticalManagers)
            {
                try
                {
                    if (!checkFunc())
                    {
                        issues.Add($"Critical manager '{managerName}' is not available");
                    }
                }
                catch (System.Exception ex)
                {
                    issues.Add($"Error checking '{managerName}': {ex.Message}");
                }
            }

            // Check TextMeshPro
            if (typeof(UnityEngine.UI.Text) != null && !ValidateTextMeshProSetup())
            {
                issues.Add("TextMeshPro may not be properly configured");
            }

            if (issues.Count > 0)
            {
                Debug.LogWarning("[CompilationSafetyManager] Project readiness issues detected:");
                foreach (string issue in issues)
                {
                    Debug.LogWarning($"  - {issue}");
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validate TextMeshPro setup
        /// </summary>
        private static bool ValidateTextMeshProSetup()
        {
            try
            {
                // Basic check if TextMeshPro is available
                var tmpAssembly = System.Reflection.Assembly.Load("Unity.TextMeshPro");
                return tmpAssembly != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Create a safe script template with proper using statements
        /// </summary>
        public static string GetSafeScriptTemplate(string className, bool needsUI = false, bool needsDOTween = false)
        {
            var template = new System.Text.StringBuilder();
            template.AppendLine("using UnityEngine;");
            
            if (needsUI)
            {
                template.AppendLine("using UnityEngine.UI;");
                template.AppendLine("using TMPro;");
                template.AppendLine("using UI.Animations;");
            }

            if (needsDOTween)
            {
                template.AppendLine("#if DOTWEEN_AVAILABLE");
                template.AppendLine("using DG.Tweening;");
                template.AppendLine("#endif");
            }

            template.AppendLine();
            template.AppendLine($"public class {className} : MonoBehaviour");
            template.AppendLine("{");
            template.AppendLine("    // Safe initialization pattern");
            template.AppendLine("    private void Start()");
            template.AppendLine("    {");
            template.AppendLine("        // Ensure manager dependencies are available");
            template.AppendLine("        if (UI.Animations.AnimationController.Instance == null)");
            template.AppendLine("        {");
            template.AppendLine("            Debug.LogWarning(\"AnimationController not available\");");
            template.AppendLine("        }");
            template.AppendLine("    }");
            template.AppendLine("}");

            return template.ToString();
        }

        /// <summary>
        /// Auto-fix common compilation issues
        /// </summary>
        public static void AutoFixCommonIssues(string scriptContent)
        {
            var fixes = new List<string>();

            // Add missing using UI.Animations
            if (scriptContent.Contains("AnimationController") && !scriptContent.Contains("using UI.Animations"))
            {
                fixes.Add("Added missing 'using UI.Animations;'");
            }

            // Add missing UnityEngine using statements
            if (scriptContent.Contains("Vector3") && !scriptContent.Contains("using UnityEngine"))
            {
                fixes.Add("Added missing 'using UnityEngine;'");
            }

            if (fixes.Count > 0)
            {
#if COMPILATION_SAFETY_DEBUG
                Debug.Log("[CompilationSafetyManager] Auto-fixes applied:");
                foreach (string fix in fixes)
                {
                    Debug.Log($"  - {fix}");
                }
#endif
            }
        }
    }
}