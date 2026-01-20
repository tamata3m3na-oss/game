using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Animations
{
    /// <summary>
    /// Central controller for all UI animations using Coroutines
    /// Provides smooth, performant animations for all UI elements
    /// </summary>
    [DefaultExecutionOrder(-145)]
    public class AnimationController : MonoBehaviour
    {
        public static AnimationController Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogWarning("[AnimationController] Instance is null. Make sure AnimationController is properly initialized.");
                }
                return instance;
            }
            private set
            {
                instance = value;
            }
        }
        
        private static AnimationController instance;

        [Header("Animation Settings")]
        [SerializeField] private float defaultFadeDuration = 0.3f;
        [SerializeField] private float defaultScaleDuration = 0.4f;
        [SerializeField] private float defaultSlideDuration = 0.5f;
        [SerializeField] private float defaultBounceDuration = 0.5f;
        
        // Animation state tracking
        private System.Collections.Generic.Dictionary<Transform, Coroutine> activeAnimations = new System.Collections.Generic.Dictionary<Transform, Coroutine>();
        
        #region Easing Functions
        private float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        private float EaseInQuad(float t)
        {
            return t * t;
        }

        private float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }

        private float EaseOutBack(float t)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private float EaseInOutSine(float t)
        {
            return -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
        }

        private float EaseLinear(float t)
        {
            return t;
        }
        #endregion
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[AnimationController] Duplicate instance detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #region Fade Animations
        public void FadeIn(CanvasGroup canvasGroup, float duration = -1, System.Action onComplete = null)
        {
            if (canvasGroup == null) return;
            
            float dur = duration > 0 ? duration : defaultFadeDuration;
            canvasGroup.alpha = 0f;
            StartCoroutine(FadeCoroutine(canvasGroup, 0f, 1f, dur, onComplete));
        }

        public void FadeOut(CanvasGroup canvasGroup, float duration = -1, System.Action onComplete = null)
        {
            if (canvasGroup == null) return;
            
            float dur = duration > 0 ? duration : defaultFadeDuration;
            StartCoroutine(FadeCoroutine(canvasGroup, 1f, 0f, dur, onComplete));
        }

        private IEnumerator FadeCoroutine(CanvasGroup group, float fromAlpha, float toAlpha, float duration, System.Action onComplete)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseOutQuad(t);
                group.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
                yield return null;
            }
            
            group.alpha = toAlpha;
            onComplete?.Invoke();
        }

        public void FadeText(TextMeshProUGUI text, float fromAlpha, float toAlpha, float duration = 0.3f)
        {
            if (text == null) return;
            StartCoroutine(FadeTextCoroutine(text, fromAlpha, toAlpha, duration));
        }

        private IEnumerator FadeTextCoroutine(TextMeshProUGUI text, float fromAlpha, float toAlpha, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseOutQuad(t);
                text.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
                yield return null;
            }
            
            text.alpha = toAlpha;
        }
        #endregion

        #region Scale Animations
        public void ScaleIn(Transform transform, float targetScale = 1f, float duration = -1, System.Action onComplete = null)
        {
            if (transform == null) return;
            
            float dur = duration > 0 ? duration : defaultScaleDuration;
            transform.localScale = Vector3.zero;
            StartCoroutine(ScaleCoroutine(transform, targetScale, dur, onComplete));
        }

        public void ScaleOut(Transform transform, float duration = -1, System.Action onComplete = null)
        {
            if (transform == null) return;
            
            float dur = duration > 0 ? duration : defaultScaleDuration;
            StartCoroutine(ScaleCoroutine(transform, 0f, dur, onComplete));
        }

        private IEnumerator ScaleCoroutine(Transform transform, float targetScale, float duration, System.Action onComplete)
        {
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 endScale = Vector3.one * targetScale;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseOutBack(t);
                transform.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }
            
            transform.localScale = endScale;
            onComplete?.Invoke();
        }

        public void Pulse(Transform transform, float scaleAmount = 1.1f, float duration = 0.5f, System.Action onComplete = null)
        {
            if (transform == null) return;
            
            StartCoroutine(PulseCoroutine(transform, scaleAmount, duration, onComplete));
        }

        private IEnumerator PulseCoroutine(Transform transform, float scaleAmount, float duration, System.Action onComplete)
        {
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * scaleAmount;
            
            // Scale up
            yield return StartCoroutine(ScaleTo(transform, targetScale, duration / 2f));
            
            // Scale down
            yield return StartCoroutine(ScaleTo(transform, originalScale, duration / 2f));
            
            onComplete?.Invoke();
        }

        private IEnumerator ScaleTo(Transform transform, Vector3 targetScale, float duration)
        {
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseOutQuad(t);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            
            transform.localScale = targetScale;
        }

        public void ContinuousPulse(Transform transform, float scaleAmount = 1.1f, float duration = 2f)
        {
            if (transform == null) return;
            
            StartCoroutine(ContinuousPulseCoroutine(transform, scaleAmount, duration));
        }

        private IEnumerator ContinuousPulseCoroutine(Transform transform, float scaleAmount, float duration)
        {
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * scaleAmount;
            
            while (true)
            {
                // Scale up
                yield return StartCoroutine(ScaleTo(transform, targetScale, duration / 2f));
                
                // Scale down
                yield return StartCoroutine(ScaleTo(transform, originalScale, duration / 2f));
            }
        }

        public void StopPulse(Transform transform)
        {
            if (transform == null) return;
            
            if (activeAnimations.ContainsKey(transform))
            {
                StopCoroutine(activeAnimations[transform]);
                activeAnimations.Remove(transform);
            }
            
            transform.localScale = Vector3.one;
        }
        #endregion

        #region Slide Animations
        public void SlideUp(Transform transform, float distance = 100f, float duration = -1, System.Action onComplete = null)
        {
            if (transform == null) return;
            
            float dur = duration > 0 ? duration : defaultSlideDuration;
            Vector3 startPos = transform.localPosition + Vector3.down * distance;
            transform.localPosition = startPos;
            StartCoroutine(SlideCoroutine(transform, startPos.y + distance, dur, onComplete));
        }

        public void SlideDown(Transform transform, float distance = 100f, float duration = -1, System.Action onComplete = null)
        {
            if (transform == null) return;
            
            float dur = duration > 0 ? duration : defaultSlideDuration;
            Vector3 startPos = transform.localPosition + Vector3.up * distance;
            transform.localPosition = startPos;
            StartCoroutine(SlideCoroutine(transform, startPos.y - distance, dur, onComplete));
        }

        public void SlideInFromLeft(Transform transform, float distance = 100f, float duration = -1, System.Action onComplete = null)
        {
            if (transform == null) return;
            
            float dur = duration > 0 ? duration : defaultSlideDuration;
            Vector3 startPos = transform.localPosition + Vector3.left * distance;
            transform.localPosition = startPos;
            StartCoroutine(SlideXCoroutine(transform, startPos.x + distance, dur, onComplete));
        }

        public void SlideOutToLeft(Transform transform, float distance = 100f, float duration = -1, System.Action onComplete = null)
        {
            if (transform == null) return;
            
            float dur = duration > 0 ? duration : defaultSlideDuration;
            StartCoroutine(SlideXCoroutine(transform, transform.localPosition.x - distance, dur, onComplete));
        }

        private IEnumerator SlideCoroutine(Transform transform, float targetY, float duration, System.Action onComplete)
        {
            float elapsed = 0f;
            float startY = transform.localPosition.y;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseOutBack(t);
                
                Vector3 pos = transform.localPosition;
                pos.y = Mathf.Lerp(startY, targetY, t);
                transform.localPosition = pos;
                yield return null;
            }
            
            Vector3 finalPos = transform.localPosition;
            finalPos.y = targetY;
            transform.localPosition = finalPos;
            onComplete?.Invoke();
        }

        private IEnumerator SlideXCoroutine(Transform transform, float targetX, float duration, System.Action onComplete)
        {
            float elapsed = 0f;
            float startX = transform.localPosition.x;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseOutQuad(t);
                
                Vector3 pos = transform.localPosition;
                pos.x = Mathf.Lerp(startX, targetX, t);
                transform.localPosition = pos;
                yield return null;
            }
            
            Vector3 finalPos = transform.localPosition;
            finalPos.x = targetX;
            transform.localPosition = finalPos;
            onComplete?.Invoke();
        }
        #endregion

        #region Shake Animations
        public void Shake(Transform transform, float strength = 5f, int vibrato = 10, float duration = 0.4f)
        {
            if (transform == null) return;
            StartCoroutine(ShakeCoroutine(transform, strength, vibrato, duration, false));
        }

        public void ShakeRotation(Transform transform, float strength = 10f, int vibrato = 10, float duration = 0.4f)
        {
            if (transform == null) return;
            StartCoroutine(ShakeRotationCoroutine(transform, strength, vibrato, duration));
        }

        private IEnumerator ShakeCoroutine(Transform transform, float strength, int vibrato, float duration, bool randomize = true)
        {
            Vector3 originalPosition = transform.localPosition;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float percentComplete = elapsed / duration;
                
                float strengthMultiplier = 1f - percentComplete;
                
                float x = Random.Range(-1f, 1f) * strength * strengthMultiplier;
                float y = Random.Range(-1f, 1f) * strength * strengthMultiplier;
                
                transform.localPosition = originalPosition + new Vector3(x, y, 0f);
                
                yield return null;
            }
            
            transform.localPosition = originalPosition;
        }

        private IEnumerator ShakeRotationCoroutine(Transform transform, float strength, int vibrato, float duration)
        {
            Quaternion originalRotation = transform.localRotation;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float percentComplete = elapsed / duration;
                
                float strengthMultiplier = 1f - percentComplete;
                
                float angle = Random.Range(-1f, 1f) * strength * strengthMultiplier;
                transform.localRotation = originalRotation * Quaternion.Euler(0f, 0f, angle);
                
                yield return null;
            }
            
            transform.localRotation = originalRotation;
        }
        #endregion

        #region Special Animations
        public void Glitch(TextMeshProUGUI text, int frames = 3, float frameDuration = 0.15f)
        {
            if (text == null) return;
            
            StartCoroutine(GlitchCoroutine(text, frames, frameDuration));
        }

        private IEnumerator GlitchCoroutine(TextMeshProUGUI text, int frames, float frameDuration)
        {
            Vector3 originalPos = text.transform.localPosition;
            Color originalColor = text.color;
            
            for (int i = 0; i < frames; i++)
            {
                text.transform.localPosition = originalPos + new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0);
                text.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                yield return new WaitForSeconds(frameDuration);
            }
            
            text.transform.localPosition = originalPos;
            text.color = originalColor;
        }

        public void CounterAnimation(TextMeshProUGUI text, int fromValue, int toValue, float duration = 0.5f)
        {
            if (text == null) return;
            
            StartCoroutine(CounterCoroutine(text, fromValue, toValue, duration));
        }

        private IEnumerator CounterCoroutine(TextMeshProUGUI text, int fromValue, int toValue, float duration)
        {
            int currentValue = fromValue;
            float elapsed = 0f;
            int totalSteps = Mathf.Abs(toValue - fromValue);
            
            if (totalSteps == 0)
            {
                text.text = toValue.ToString();
                yield break;
            }
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                currentValue = Mathf.RoundToInt(Mathf.Lerp(fromValue, toValue, EaseOutQuad(t)));
                text.text = currentValue.ToString();
                yield return null;
            }
            
            text.text = toValue.ToString();
        }

        public void FloatAnimation(Transform transform, float amplitude = 10f, float duration = 2f)
        {
            if (transform == null) return;
            
            StartCoroutine(FloatCoroutine(transform, amplitude, duration));
        }

        private IEnumerator FloatCoroutine(Transform transform, float amplitude, float duration)
        {
            Vector3 originalPosition = transform.localPosition;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float offset = Mathf.Sin(elapsed * Mathf.PI * 2f / duration) * amplitude;
                transform.localPosition = originalPosition + new Vector3(0f, offset, 0f);
                yield return null;
            }
            
            transform.localPosition = originalPosition;
        }

        public void RotateAnimation(Transform transform, float duration = 6f)
        {
            if (transform == null) return;
            
            StartCoroutine(RotateCoroutine(transform, duration));
        }

        private IEnumerator RotateCoroutine(Transform transform, float duration)
        {
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.Rotate(Vector3.forward, 360f * Time.deltaTime / duration);
                yield return null;
            }
        }

        public void StopRotateAnimation(Transform transform)
        {
            if (transform == null) return;
            
            // Stop all coroutines for this transform
            if (activeAnimations.ContainsKey(transform))
            {
                StopCoroutine(activeAnimations[transform]);
                activeAnimations.Remove(transform);
            }
        }
        #endregion

        #region Button Press Animations
        public void ButtonPress(Transform transform, System.Action onComplete = null)
        {
            if (transform == null) return;
            
            StartCoroutine(ButtonPressCoroutine(transform, onComplete));
        }

        private IEnumerator ButtonPressCoroutine(Transform transform, System.Action onComplete)
        {
            // Scale down
            yield return StartCoroutine(ScaleTo(transform, Vector3.one * 0.95f, 0.1f));
            
            // Scale back up
            yield return StartCoroutine(ScaleTo(transform, Vector3.one, 0.1f));
            
            onComplete?.Invoke();
        }

        public void ButtonHover(Transform transform, bool isHovering)
        {
            if (transform == null) return;
            
            Vector3 targetScale = isHovering ? Vector3.one * 1.05f : Vector3.one;
            StartCoroutine(ScaleTo(transform, targetScale, 0.2f));
        }
        #endregion

        #region Health Bar Animations
        public void AnimateHealthBar(Image healthBar, float fromValue, float toValue, float duration = 0.2f)
        {
            if (healthBar == null) return;
            
            StartCoroutine(HealthBarCoroutine(healthBar, fromValue, toValue, duration));
        }

        private IEnumerator HealthBarCoroutine(Image healthBar, float fromValue, float toValue, float duration)
        {
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseOutQuad(t);
                healthBar.fillAmount = Mathf.Lerp(fromValue, toValue, t);
                yield return null;
            }
            
            healthBar.fillAmount = toValue;
        }

        public void DamageFlash(Image flashOverlay, float duration = 0.1f)
        {
            if (flashOverlay == null) return;
            
            StartCoroutine(DamageFlashCoroutine(flashOverlay, duration));
        }

        private IEnumerator DamageFlashCoroutine(Image flashOverlay, float duration)
        {
            flashOverlay.color = new Color(1f, 0f, 0f, 0.5f);
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseOutQuad(t);
                flashOverlay.color = new Color(1f, 0f, 0f, Mathf.Lerp(0.5f, 0f, t));
                yield return null;
            }
            
            flashOverlay.color = Color.clear;
        }
        #endregion

        #region Staggered Animations
        public void StaggeredScaleIn(Transform[] transforms, float delayBetween = 0.1f, float duration = 0.5f)
        {
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] != null)
                {
                    float delay = i * delayBetween;
                    StartCoroutine(DelayedScaleIn(transforms[i], delay, duration));
                }
            }
        }

        private IEnumerator DelayedScaleIn(Transform transform, float delay, float duration)
        {
            yield return new WaitForSeconds(delay);
            ScaleIn(transform, 1f, duration, null);
        }

        public void StaggeredFadeIn(CanvasGroup[] canvasGroups, float delayBetween = 0.05f, float duration = 0.3f)
        {
            for (int i = 0; i < canvasGroups.Length; i++)
            {
                if (canvasGroups[i] != null)
                {
                    float delay = i * delayBetween;
                    StartCoroutine(DelayedFadeIn(canvasGroups[i], delay, duration));
                }
            }
        }

        private IEnumerator DelayedFadeIn(CanvasGroup canvasGroup, float delay, float duration)
        {
            yield return new WaitForSeconds(delay);
            FadeIn(canvasGroup, duration, null);
        }

        public void StaggeredSlideUp(Transform[] transforms, float distance = 50f, float delayBetween = 0.1f, float duration = 0.5f)
        {
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] != null)
                {
                    float delay = i * delayBetween;
                    StartCoroutine(DelayedSlideUp(transforms[i], delay, distance, duration));
                }
            }
        }

        private IEnumerator DelayedSlideUp(Transform transform, float delay, float distance, float duration)
        {
            yield return new WaitForSeconds(delay);
            SlideUp(transform, distance, duration, null);
        }
        #endregion

        #region Cleanup
        private void OnDestroy()
        {
            // Stop all running coroutines
            StopAllCoroutines();
        }
        #endregion
    }
}