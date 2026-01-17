using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace UI.Animations
{
    /// <summary>
    /// Central controller for all UI animations using DOTween
    /// Provides smooth, performant animations for all UI elements
    /// </summary>
    public class AnimationController : MonoBehaviour
    {
        public static AnimationController Instance { get; private set; }

        [Header("Animation Settings")]
        [SerializeField] private float defaultFadeDuration = 0.3f;
        [SerializeField] private float defaultScaleDuration = 0.4f;
        [SerializeField] private float defaultSlideDuration = 0.5f;
        [SerializeField] private float defaultBounceDuration = 0.5f;
        
        [SerializeField] private Ease defaultEase = Ease.OutQuad;
        [SerializeField] private Ease bounceEase = Ease.OutBack;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region Fade Animations
        public void FadeIn(CanvasGroup canvasGroup, float duration = -1, System.Action onComplete = null)
        {
            if (canvasGroup == null) return;
            
            float dur = duration > 0 ? duration : defaultFadeDuration;
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, dur).SetEase(defaultEase).OnComplete(() => onComplete?.Invoke());
        }

        public void FadeOut(CanvasGroup canvasGroup, float duration = -1, System.Action onComplete = null)
        {
            if (canvasGroup == null) return;
            
            float dur = duration > 0 ? duration : defaultFadeDuration;
            canvasGroup.DOFade(0f, dur).SetEase(defaultEase).OnComplete(() => onComplete?.Invoke());
        }

        public void FadeText(TextMeshProUGUI text, float fromAlpha, float toAlpha, float duration = 0.3f)
        {
            if (text == null) return;
            text.alpha = fromAlpha;
            text.DOFade(toAlpha, duration).SetEase(defaultEase);
        }
        #endregion

        #region Scale Animations
        public void ScaleIn(Transform transform, float targetScale = 1f, float duration = -1, System.Action onComplete = null)
        {
            if (transform == null) return;
            
            float dur = duration > 0 ? duration : defaultScaleDuration;
            transform.localScale = Vector3.zero;
            transform.DOScale(targetScale, dur).SetEase(bounceEase).OnComplete(() => onComplete?.Invoke());
        }

        public void ScaleOut(Transform transform, float duration = -1, System.Action onComplete = null)
        {
            if (transform == null) return;
            
            float dur = duration > 0 ? duration : defaultScaleDuration;
            transform.DOScale(0f, dur).SetEase(defaultEase).OnComplete(() => onComplete?.Invoke());
        }

        public void Pulse(Transform transform, float scaleAmount = 1.1f, float duration = 0.5f, System.Action onComplete = null)
        {
            if (transform == null) return;
            
            transform.DOScale(scaleAmount, duration / 2)
                .SetEase(defaultEase)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void ContinuousPulse(Transform transform, float scaleAmount = 1.1f, float duration = 2f)
        {
            if (transform == null) return;
            
            transform.DOScale(scaleAmount, duration / 2)
                .SetEase(defaultEase)
                .SetLoops(-1, LoopType.Yoyo);
        }

        public void StopPulse(Transform transform)
        {
            if (transform == null) return;
            transform.DOKill();
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
            transform.DOLocalMoveY(startPos.y + distance, dur).SetEase(bounceEase).OnComplete(() => onComplete?.Invoke());
        }

        public void SlideDown(Transform transform, float distance = 100f, float duration = -1, System.Action onComplete = null)
        {
            if (transform == null) return;
            
            float dur = duration > 0 ? duration : defaultSlideDuration;
            Vector3 startPos = transform.localPosition + Vector3.up * distance;
            transform.localPosition = startPos;
            transform.DOLocalMoveY(startPos.y - distance, dur).SetEase(defaultEase).OnComplete(() => onComplete?.Invoke());
        }

        public void SlideInFromLeft(Transform transform, float distance = 100f, float duration = -1, System.Action onComplete = null)
        {
            if (transform == null) return;
            
            float dur = duration > 0 ? duration : defaultSlideDuration;
            Vector3 startPos = transform.localPosition + Vector3.left * distance;
            transform.localPosition = startPos;
            transform.DOLocalMoveX(startPos.x + distance, dur).SetEase(defaultEase).OnComplete(() => onComplete?.Invoke());
        }

        public void SlideOutToLeft(Transform transform, float distance = 100f, float duration = -1, System.Action onComplete = null)
        {
            if (transform == null) return;
            
            float dur = duration > 0 ? duration : defaultSlideDuration;
            transform.DOLocalMoveX(transform.localPosition.x - distance, dur).SetEase(defaultEase).OnComplete(() => onComplete?.Invoke());
        }
        #endregion

        #region Shake Animations
        public void Shake(Transform transform, float strength = 5f, int vibrato = 10, float duration = 0.4f)
        {
            if (transform == null) return;
            transform.DOShakePosition(duration, strength, vibrato, 90, false, true);
        }

        public void ShakeRotation(Transform transform, float strength = 10f, int vibrato = 10, float duration = 0.4f)
        {
            if (transform == null) return;
            transform.DOShakeRotation(duration, strength, vibrato, 90, false);
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
            
            int currentValue = fromValue;
            float updateInterval = duration / Mathf.Abs(toValue - fromValue);
            
            DOTween.To(() => currentValue, x => 
            {
                currentValue = x;
                text.text = currentValue.ToString();
            }, toValue, duration).SetEase(Ease.OutQuad);
        }

        public void FloatAnimation(Transform transform, float amplitude = 10f, float duration = 2f)
        {
            if (transform == null) return;
            
            transform.DOBlendableMoveBy(Vector3.up * amplitude, duration / 2)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        public void RotateAnimation(Transform transform, float duration = 6f, RotateMode mode = RotateMode.WorldAxisAdd)
        {
            if (transform == null) return;
            
            transform.DORotate(Vector3.forward * 360, duration, mode).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
        }

        public void StopRotateAnimation(Transform transform)
        {
            if (transform == null) return;
            transform.DOKill();
        }
        #endregion

        #region Button Press Animations
        public void ButtonPress(Transform transform, System.Action onComplete = null)
        {
            if (transform == null) return;
            
            transform.DOScale(0.95f, 0.1f).SetEase(defaultEase).OnComplete(() => 
            {
                transform.DOScale(1f, 0.1f).SetEase(defaultEase).OnComplete(() => onComplete?.Invoke());
            });
        }

        public void ButtonHover(Transform transform, bool isHovering)
        {
            if (transform == null) return;
            
            if (isHovering)
            {
                transform.DOScale(1.05f, 0.2f).SetEase(defaultEase);
            }
            else
            {
                transform.DOScale(1f, 0.2f).SetEase(defaultEase);
            }
        }
        #endregion

        #region Health Bar Animations
        public void AnimateHealthBar(Image healthBar, float fromValue, float toValue, float duration = 0.2f)
        {
            if (healthBar == null) return;
            
            healthBar.DOFillAmount(toValue, duration).SetEase(Ease.OutQuad);
        }

        public void DamageFlash(Image flashOverlay, float duration = 0.1f)
        {
            if (flashOverlay == null) return;
            
            flashOverlay.color = new Color(1f, 0f, 0f, 0.5f);
            flashOverlay.DOFade(0f, duration).SetEase(Ease.OutQuad);
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
                    ScaleIn(transforms[i], 1f, duration, null);
                    transforms[i].DOScale(0f, 0f).SetDelay(delay).OnComplete(() => 
                    {
                        transforms[i].DOScale(1f, duration).SetEase(bounceEase);
                    });
                }
            }
        }

        public void StaggeredFadeIn(CanvasGroup[] canvasGroups, float delayBetween = 0.05f, float duration = 0.3f)
        {
            for (int i = 0; i < canvasGroups.Length; i++)
            {
                if (canvasGroups[i] != null)
                {
                    float delay = i * delayBetween;
                    canvasGroups[i].alpha = 0f;
                    canvasGroups[i].DOFade(1f, duration).SetEase(defaultEase).SetDelay(delay);
                }
            }
        }

        public void StaggeredSlideUp(Transform[] transforms, float distance = 50f, float delayBetween = 0.1f, float duration = 0.5f)
        {
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] != null)
                {
                    float delay = i * delayBetween;
                    Vector3 startPos = transforms[i].localPosition + Vector3.down * distance;
                    transforms[i].localPosition = startPos;
                    transforms[i].DOLocalMoveY(startPos.y + distance, duration).SetEase(bounceEase).SetDelay(delay);
                }
            }
        }
        #endregion

        #region Cleanup
        private void OnDestroy()
        {
            DOTween.KillAll();
        }
        #endregion
    }
}
