#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define DOTWEEN_DEBUG
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UI.Animations.DOTweenCompat
{
    /// <summary>
    /// DOTween Compatibility Layer
    /// Provides safe DOTween usage with graceful fallbacks
    /// Prevents compilation errors when DOTween is not available
    /// </summary>
    public static class DOTweenCompat
    {
        private static bool isDOTweenAvailable = false;
        private static bool hasCheckedAvailability = false;

        /// <summary>
        /// Checks if DOTween is available in the project
        /// </summary>
        public static bool IsDOTweenAvailable()
        {
            if (!hasCheckedAvailability)
            {
                CheckDOTweenAvailability();
                hasCheckedAvailability = true;
            }
            
            return isDOTweenAvailable;
        }

        private static void CheckDOTweenAvailability()
        {
            try
            {
#if DOTWEEN_AVAILABLE
                // Check if DOTween namespace is available
                var tweenType = System.Type.GetType("DG.Tweening.Tween, DOTween");
                isDOTweenAvailable = tweenType != null;
#else
                isDOTweenAvailable = false;
#endif

#if DOTWEEN_DEBUG
                Debug.Log($"[DOTweenCompat] DOTween availability check: {isDOTweenAvailable}");
#endif
            }
            catch (System.Exception ex)
            {
                isDOTweenAvailable = false;
#if DOTWEEN_DEBUG
                Debug.LogWarning($"[DOTweenCompat] Error checking DOTween availability: {ex.Message}");
#endif
            }
        }

        /// <summary>
        /// Safe wrapper for DOTween tweens with Coroutine fallback
        /// </summary>
        public static void SafeTween(Transform target, Vector3 to, float duration, System.Action onComplete = null)
        {
            if (!IsDOTweenAvailable())
            {
                // Fallback to Coroutine-based animation
                target.StartCoroutine(SafeTweenCoroutine(target, to, duration, onComplete));
                return;
            }

#if DOTWEEN_AVAILABLE
            try
            {
                // Use actual DOTween if available
                DG.Tweening.DOTween.To(() => target.position, x => target.position = x, to, duration);
            }
            catch (System.Exception ex)
            {
#if DOTWEEN_DEBUG
                Debug.LogWarning($"[DOTweenCompat] DOTween failed, using fallback: {ex.Message}");
#endif
                target.StartCoroutine(SafeTweenCoroutine(target, to, duration, onComplete));
            }
#endif
        }

        /// <summary>
        /// Safe wrapper for scale tweens
        /// </summary>
        public static void SafeScaleTween(Transform target, Vector3 to, float duration, System.Action onComplete = null)
        {
            if (!IsDOTweenAvailable())
            {
                target.StartCoroutine(SafeScaleTweenCoroutine(target, to, duration, onComplete));
                return;
            }

#if DOTWEEN_AVAILABLE
            try
            {
                DG.Tweening.DOTween.To(() => target.localScale, x => target.localScale = x, to, duration);
            }
            catch (System.Exception ex)
            {
#if DOTWEEN_DEBUG
                Debug.LogWarning($"[DOTweenCompat] DOTween scale failed, using fallback: {ex.Message}");
#endif
                target.StartCoroutine(SafeScaleTweenCoroutine(target, to, duration, onComplete));
            }
#endif
        }

        /// <summary>
        /// Safe wrapper for rotation tweens
        /// </summary>
        public static void SafeRotateTween(Transform target, Vector3 to, float duration, System.Action onComplete = null)
        {
            if (!IsDOTweenAvailable())
            {
                target.StartCoroutine(SafeRotateTweenCoroutine(target, to, duration, onComplete));
                return;
            }

#if DOTWEEN_AVAILABLE
            try
            {
                DG.Tweening.DOTween.To(() => target.rotation.eulerAngles, x => target.rotation = Quaternion.Euler(x), to, duration);
            }
            catch (System.Exception ex)
            {
#if DOTWEEN_DEBUG
                Debug.LogWarning($"[DOTweenCompat] DOTween rotation failed, using fallback: {ex.Message}");
#endif
                target.StartCoroutine(SafeRotateTweenCoroutine(target, to, duration, onComplete));
            }
#endif
        }

        /// <summary>
        /// Safe wrapper for fade tweens on CanvasGroup
        /// </summary>
        public static void SafeFadeTween(CanvasGroup target, float to, float duration, System.Action onComplete = null)
        {
            if (!IsDOTweenAvailable())
            {
                target.StartCoroutine(SafeFadeTweenCoroutine(target, to, duration, onComplete));
                return;
            }

#if DOTWEEN_AVAILABLE
            try
            {
                DG.Tweening.DOTween.To(() => target.alpha, x => target.alpha = x, to, duration);
            }
            catch (System.Exception ex)
            {
#if DOTWEEN_DEBUG
                Debug.LogWarning($"[DOTweenCompat] DOTween fade failed, using fallback: {ex.Message}");
#endif
                target.StartCoroutine(SafeFadeTweenCoroutine(target, to, duration, onComplete));
            }
#endif
        }

        #region Coroutine Fallbacks

        private static IEnumerator SafeTweenCoroutine(Transform target, Vector3 to, float duration, System.Action onComplete)
        {
            Vector3 from = target.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                target.position = Vector3.Lerp(from, to, t);
                yield return null;
            }

            target.position = to;
            onComplete?.Invoke();
        }

        private static IEnumerator SafeScaleTweenCoroutine(Transform target, Vector3 to, float duration, System.Action onComplete)
        {
            Vector3 from = target.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                target.localScale = Vector3.Lerp(from, to, t);
                yield return null;
            }

            target.localScale = to;
            onComplete?.Invoke();
        }

        private static IEnumerator SafeRotateTweenCoroutine(Transform target, Vector3 to, float duration, System.Action onComplete)
        {
            Quaternion from = target.rotation;
            Quaternion toRot = Quaternion.Euler(to);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                target.rotation = Quaternion.Slerp(from, toRot, t);
                yield return null;
            }

            target.rotation = toRot;
            onComplete?.Invoke();
        }

        private static IEnumerator SafeFadeTweenCoroutine(CanvasGroup target, float to, float duration, System.Action onComplete)
        {
            float from = target.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                target.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            target.alpha = to;
            onComplete?.Invoke();
        }

        #endregion

        /// <summary>
        /// Kills all active tweens on a specific target
        /// </summary>
        public static void SafeKill(Transform target, bool complete = false)
        {
            if (!IsDOTweenAvailable())
            {
                // No way to stop specific coroutines from here, but that's okay
                return;
            }

#if DOTWEEN_AVAILABLE
            try
            {
                if (complete)
                {
                    DG.Tweening.DOTween.Complete(target);
                }
                else
                {
                    DG.Tweening.DOTween.Kill(target);
                }
            }
            catch (System.Exception ex)
            {
#if DOTWEEN_DEBUG
                Debug.LogWarning($"[DOTweenCompat] Error killing tweens: {ex.Message}");
#endif
            }
#endif
        }

        /// <summary>
        /// Kills all active tweens globally
        /// </summary>
        public static void SafeKillAll(bool complete = false)
        {
            if (!IsDOTweenAvailable())
            {
                return;
            }

#if DOTWEEN_AVAILABLE
            try
            {
                if (complete)
                {
                    DG.Tweening.DOTween.CompleteAll();
                }
                else
                {
                    DG.Tweening.DOTween.KillAll();
                }
            }
            catch (System.Exception ex)
            {
#if DOTWEEN_DEBUG
                Debug.LogWarning($"[DOTweenCompat] Error killing all tweens: {ex.Message}");
#endif
            }
#endif
        }

        /// <summary>
        /// Returns the active tween count
        /// </summary>
        public static int GetActiveTweenCount()
        {
            if (!IsDOTweenAvailable())
            {
                return 0; // We can't track coroutines easily
            }

#if DOTWEEN_AVAILABLE
            try
            {
                return DG.Tweening.DOTween.TotalActiveTweens();
            }
            catch (System.Exception ex)
            {
#if DOTWEEN_DEBUG
                Debug.LogWarning($"[DOTweenCompat] Error getting tween count: {ex.Message}");
#endif
                return 0;
            }
#else
            return 0;
#endif
        }
    }
}

#if DOTWEEN_AVAILABLE
// Safe extension methods for easy usage
public static class DOTweenExtensions
{
    public static void SafeMoveTo(this Transform target, Vector3 position, float duration, System.Action onComplete = null)
    {
        UI.Animations.DOTweenCompat.DOTweenCompat.SafeTween(target, position, duration, onComplete);
    }

    public static void SafeScaleTo(this Transform target, Vector3 scale, float duration, System.Action onComplete = null)
    {
        UI.Animations.DOTweenCompat.DOTweenCompat.SafeScaleTween(target, scale, duration, onComplete);
    }

    public static void SafeRotateTo(this Transform target, Vector3 rotation, float duration, System.Action onComplete = null)
    {
        UI.Animations.DOTweenCompat.DOTweenCompat.SafeRotateTween(target, rotation, duration, onComplete);
    }

    public static void SafeFadeTo(this UnityEngine.CanvasGroup target, float alpha, float duration, System.Action onComplete = null)
    {
        UI.Animations.DOTweenCompat.DOTweenCompat.SafeFadeTween(target, alpha, duration, onComplete);
    }

    public static void SafeKill(this Transform target, bool complete = false)
    {
        UI.Animations.DOTweenCompat.DOTweenCompat.SafeKill(target, complete);
    }
}
#endif