using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace UI.Effects
{
    /// <summary>
    /// Adds customizable glow effects to UI elements
    /// Supports pulse, breathe, and static glow effects
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public class GlowEffect : MonoBehaviour
    {
        [Header("Glow Settings")]
        [SerializeField] private Color glowColor = new Color(0f, 0.83f, 1f, 1f); // Cyan default
        [SerializeField] private float glowIntensity = 1f;
        [SerializeField] private GlowMode glowMode = GlowMode.None;

        [Header("Animation Settings")]
        [SerializeField] private float pulseSpeed = 1f;
        [SerializeField] private float minIntensity = 0.5f;
        [SerializeField] private float maxIntensity = 1.5f;

        [Header("Shadow Settings")]
        [SerializeField] private bool enableShadow = true;
        [SerializeField] private Vector2 shadowOffset = new Vector2(0f, -2f);
        [SerializeField] private Color shadowColor = new Color(0f, 0.83f, 1f, 0.5f);

        private Graphic targetGraphic;
        private Shadow shadowComponent;
        private Coroutine pulseCoroutine;
        private Color originalColor;
        private bool isHovered = false;

        private void Awake()
        {
            targetGraphic = GetComponent<Graphic>();
            originalColor = targetGraphic.color;

            SetupShadow();
        }

        private void Start()
        {
            ApplyGlowMode();
        }

        private void SetupShadow()
        {
            if (enableShadow)
            {
                shadowComponent = GetComponent<Shadow>();
                if (shadowComponent == null)
                {
                    shadowComponent = gameObject.AddComponent<Shadow>();
                }
                
                shadowComponent.effectColor = shadowColor;
                shadowComponent.effectDistance = shadowOffset;
            }
        }

        public void SetGlowMode(GlowMode mode)
        {
            glowMode = mode;
            ApplyGlowMode();
        }

        private void ApplyGlowMode()
        {
            StopPulse();

            switch (glowMode)
            {
                case GlowMode.Static:
                    ApplyStaticGlow();
                    break;
                case GlowMode.Pulse:
                    StartPulse();
                    break;
                case GlowMode.Breathe:
                    StartBreathe();
                    break;
                case GlowMode.OnHover:
                    ApplyStaticGlow(false);
                    break;
                case GlowMode.None:
                    RemoveGlow();
                    break;
            }
        }

        public void ApplyStaticGlow(bool show = true)
        {
            if (show)
            {
                if (shadowComponent != null)
                {
                    shadowComponent.enabled = true;
                    shadowComponent.effectColor = glowColor * glowIntensity;
                }
            }
            else
            {
                RemoveGlow();
            }
        }

        public void StartPulse()
        {
            if (shadowComponent == null) return;

            shadowComponent.enabled = true;

            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
            }

            pulseCoroutine = StartCoroutine(PulseCoroutine());
        }

        private IEnumerator PulseCoroutine()
        {
            float time = 0f;
            
            while (glowMode == GlowMode.Pulse)
            {
                time += Time.deltaTime * pulseSpeed;
                
                // Calculate pulsing alpha using sine wave
                float alpha = (Mathf.Sin(time) + 1f) / 2f; // Convert to 0-1 range
                alpha = Mathf.Lerp(minIntensity, maxIntensity, alpha);
                
                if (shadowComponent != null)
                {
                    shadowComponent.effectColor = new Color(glowColor.r, glowColor.g, glowColor.b, alpha * glowColor.a);
                }
                
                yield return null;
            }
        }

        public void StartBreathe()
        {
            if (targetGraphic == null) return;

            // Breathe effect on the element itself
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
            }

            pulseCoroutine = StartCoroutine(BreatheCoroutine());

            // Also pulse the glow
            StartPulse();
        }

        private IEnumerator BreatheCoroutine()
        {
            float time = 0f;
            Vector3 originalScale = transform.localScale;
            
            while (glowMode == GlowMode.Breathe)
            {
                time += Time.deltaTime * pulseSpeed;
                
                // Calculate breathing scale using sine wave
                float scale = 1f + 0.05f * Mathf.Sin(time * 2f);
                transform.localScale = originalScale * scale;
                
                yield return null;
            }
            
            transform.localScale = originalScale;
        }

        public void StopPulse()
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }

            transform.localScale = Vector3.one;
        }

        public void RemoveGlow()
        {
            if (shadowComponent != null)
            {
                shadowComponent.enabled = false;
            }
            
            StopPulse();
        }

        public void SetGlowColor(Color color)
        {
            glowColor = color;
            if (shadowComponent != null)
            {
                shadowComponent.effectColor = color * glowIntensity;
            }
        }

        public void SetGlowIntensity(float intensity)
        {
            glowIntensity = intensity;
            ApplyStaticGlow();
        }

        public void FlashGlow(float duration = 0.3f)
        {
            if (shadowComponent == null) return;

            StartCoroutine(FlashGlowCoroutine(duration));
        }

        private IEnumerator FlashGlowCoroutine(float duration)
        {
            Color original = shadowComponent.effectColor;
            Color flashColor = glowColor * 2f;

            float halfDuration = duration / 2f;
            
            // Flash up
            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                float easedT = EaseOutQuad(t);
                
                Color current = Color.Lerp(original, flashColor, easedT);
                shadowComponent.effectColor = current;
                
                yield return null;
            }
            
            // Flash down
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                float easedT = EaseInQuad(t);
                
                Color current = Color.Lerp(flashColor, original * glowIntensity, easedT);
                shadowComponent.effectColor = current;
                
                yield return null;
            }
            
            shadowComponent.effectColor = original * glowIntensity;
        }

        #region Easing Functions
        private float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        private float EaseInQuad(float t)
        {
            return t * t;
        }
        #endregion

        #region Hover Events
        public void OnHoverEnter()
        {
            if (glowMode == GlowMode.OnHover)
            {
                ApplyStaticGlow(true);
                isHovered = true;
            }
        }

        public void OnHoverExit()
        {
            if (glowMode == GlowMode.OnHover)
            {
                ApplyStaticGlow(false);
                isHovered = false;
            }
        }
        #endregion

        #region Preset Colors
        public static Color CyanGlow => new Color(0f, 0.83f, 1f, 1f);
        public static Color MagentaGlow => new Color(1f, 0f, 0.43f, 1f);
        public static Color GreenGlow => new Color(0f, 1f, 0.53f, 1f);
        public static Color RedGlow => new Color(1f, 0.27f, 0.27f, 1f);
        public static Color OrangeGlow => new Color(1f, 0.72f, 0f, 1f);
        public static Color YellowGlow => new Color(1f, 0.92f, 0f, 1f);
        #endregion

        private void OnDestroy()
        {
            StopPulse();
        }
    }

    public enum GlowMode
    {
        None,
        Static,
        Pulse,
        Breathe,
        OnHover
    }

    /// <summary>
    /// Helper component for detecting hover events on UI elements
    /// Attach this to UI elements that need hover glow effects
    /// </summary>
    public class UIHoverDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private GlowEffect glowEffect;

        private void Awake()
        {
            glowEffect = GetComponent<GlowEffect>();
            if (glowEffect == null)
            {
                glowEffect = GetComponentInParent<GlowEffect>();
            }
        }

        public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
        {
            glowEffect?.OnHoverEnter();
        }

        public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
        {
            glowEffect?.OnHoverExit();
        }
    }
}