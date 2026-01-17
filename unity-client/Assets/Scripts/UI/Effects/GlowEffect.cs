using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

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
        private Tween pulseTween;
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

            if (pulseTween != null && pulseTween.IsActive())
            {
                pulseTween.Kill();
            }

            pulseTween = DOTween.To(() => shadowComponent.effectColor.a,
                x => shadowComponent.effectColor = new Color(glowColor.r, glowColor.g, glowColor.b, x),
                maxIntensity * glowColor.a, pulseSpeed)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .OnStepComplete(() =>
                {
                    // Toggle between min and max intensity
                    if (shadowComponent.effectColor.a >= maxIntensity * glowColor.a - 0.01f)
                    {
                        // Going down
                    }
                });
        }

        public void StartBreathe()
        {
            if (targetGraphic == null) return;

            // Breathe effect on the element itself
            transform.DOScale(1.05f, pulseSpeed)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);

            // Also pulse the glow
            StartPulse();
        }

        public void StopPulse()
        {
            if (pulseTween != null && pulseTween.IsActive())
            {
                pulseTween.Kill();
            }

            transform.DOKill();
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

            Color original = shadowComponent.effectColor;
            Color flashColor = glowColor * 2f;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(DOTween.To(() => shadowComponent.effectColor.a,
                x => shadowComponent.effectColor = new Color(flashColor.r, flashColor.g, flashColor.b, x),
                1f, duration / 2).SetEase(Ease.OutQuad));
            sequence.Append(DOTween.To(() => shadowComponent.effectColor.a,
                x => shadowComponent.effectColor = new Color(original.r, original.g, original.b, x),
                glowIntensity, duration / 2).SetEase(Ease.InQuad));
        }

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
