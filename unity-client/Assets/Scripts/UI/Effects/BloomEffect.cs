using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace UI.Effects
{
    /// <summary>
    /// Simulated bloom effect for UI elements using multiple shadow layers
    /// Creates a glowing, neon-style appearance
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public class BloomEffect : MonoBehaviour
    {
        [Header("Bloom Settings")]
        [SerializeField] private Color bloomColor = new Color(0f, 0.83f, 1f, 1f);
        [SerializeField] private int bloomLayers = 3;
        [SerializeField] private float bloomIntensity = 1f;
        [SerializeField] private float bloomSpread = 5f;

        [Header("Animation")]
        [SerializeField] private bool animateBloom = false;
        [SerializeField] private float pulseSpeed = 1f;
        [SerializeField] private float minIntensity = 0.5f;
        [SerializeField] private float maxIntensity = 1.5f;

        private Graphic targetGraphic;
        private Shadow[] bloomShadows;
        private Coroutine pulseCoroutine;
        private Color originalColor;

        private void Awake()
        {
            targetGraphic = GetComponent<Graphic>();
            originalColor = targetGraphic.color;

            SetupBloom();
        }

        private void SetupBloom()
        {
            // Remove existing bloom shadows
            RemoveBloom();

            // Create bloom shadows
            bloomShadows = new Shadow[bloomLayers];
            
            for (int i = 0; i < bloomLayers; i++)
            {
                bloomShadows[i] = gameObject.AddComponent<Shadow>();
                
                // Calculate layer properties
                float layerSpread = bloomSpread * (i + 1);
                float layerAlpha = (1f - (i / (float)bloomLayers)) * 0.5f * bloomIntensity;
                
                bloomShadows[i].effectColor = new Color(bloomColor.r, bloomColor.g, bloomColor.b, layerAlpha);
                bloomShadows[i].effectDistance = new Vector2(layerSpread, -layerSpread);
            }

            if (animateBloom)
            {
                StartPulse();
            }
        }

        public void StartPulse()
        {
            if (bloomShadows == null || bloomShadows.Length == 0) return;

            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
            }

            pulseCoroutine = StartCoroutine(PulseCoroutine());
        }

        private IEnumerator PulseCoroutine()
        {
            float time = 0f;
            
            while (animateBloom)
            {
                time += Time.deltaTime * pulseSpeed;
                
                // Calculate pulsing intensity using sine wave
                float intensity = (Mathf.Sin(time) + 1f) / 2f; // Convert to 0-1 range
                intensity = Mathf.Lerp(minIntensity, maxIntensity, intensity);
                
                UpdateBloomIntensity(intensity);
                
                yield return null;
            }
        }

        private void UpdateBloomIntensity(float intensity)
        {
            bloomIntensity = intensity;
            
            for (int i = 0; i < bloomLayers; i++)
            {
                if (bloomShadows[i] != null)
                {
                    float layerAlpha = (1f - (i / (float)bloomLayers)) * 0.5f * bloomIntensity;
                    bloomShadows[i].effectColor = new Color(bloomColor.r, bloomColor.g, bloomColor.b, layerAlpha);
                }
            }
        }

        public void StopPulse()
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }
        }

        public void SetBloomColor(Color color)
        {
            bloomColor = color;
            
            for (int i = 0; i < bloomShadows.Length; i++)
            {
                if (bloomShadows[i] != null)
                {
                    float layerAlpha = (1f - (i / (float)bloomLayers)) * 0.5f * bloomIntensity;
                    bloomShadows[i].effectColor = new Color(color.r, color.g, color.b, layerAlpha);
                }
            }
        }

        public void SetBloomIntensity(float intensity)
        {
            bloomIntensity = intensity;
            UpdateBloomIntensity(intensity);
        }

        public void SetBloomSpread(float spread)
        {
            bloomSpread = spread;
            
            for (int i = 0; i < bloomLayers; i++)
            {
                if (bloomShadows[i] != null)
                {
                    float layerSpread = bloomSpread * (i + 1);
                    bloomShadows[i].effectDistance = new Vector2(layerSpread, -layerSpread);
                }
            }
        }

        public void FlashBloom(float duration = 0.3f, float flashIntensity = 2f)
        {
            StartCoroutine(FlashBloomCoroutine(duration, flashIntensity));
        }

        private IEnumerator FlashBloomCoroutine(float duration, float flashIntensity)
        {
            float originalIntensity = bloomIntensity;
            float halfDuration = duration / 2f;
            
            // Flash up
            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                float easedT = EaseOutQuad(t);
                float currentIntensity = Mathf.Lerp(originalIntensity, flashIntensity, easedT);
                
                UpdateBloomIntensity(currentIntensity);
                yield return null;
            }
            
            // Flash down
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                float easedT = EaseInQuad(t);
                float currentIntensity = Mathf.Lerp(flashIntensity, originalIntensity, easedT);
                
                UpdateBloomIntensity(currentIntensity);
                yield return null;
            }
            
            UpdateBloomIntensity(originalIntensity);
        }

        public void RemoveBloom()
        {
            if (bloomShadows != null)
            {
                foreach (Shadow shadow in bloomShadows)
                {
                    if (shadow != null)
                    {
                        Destroy(shadow);
                    }
                }
                bloomShadows = null;
            }
        }

        public void EnableBloom()
        {
            if (bloomShadows == null)
            {
                SetupBloom();
            }
            else
            {
                foreach (Shadow shadow in bloomShadows)
                {
                    if (shadow != null)
                    {
                        shadow.enabled = true;
                    }
                }
            }
        }

        public void DisableBloom()
        {
            if (bloomShadows != null)
            {
                foreach (Shadow shadow in bloomShadows)
                {
                    if (shadow != null)
                    {
                        shadow.enabled = false;
                    }
                }
            }
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

        #region Preset Colors
        public void SetCyanBloom()
        {
            SetBloomColor(new Color(0f, 0.83f, 1f, 1f));
        }

        public void SetMagentaBloom()
        {
            SetBloomColor(new Color(1f, 0f, 0.43f, 1f));
        }

        public void SetGreenBloom()
        {
            SetBloomColor(new Color(0f, 1f, 0.53f, 1f));
        }

        public void SetRedBloom()
        {
            SetBloomColor(new Color(1f, 0.27f, 0.27f, 1f));
        }

        public void SetOrangeBloom()
        {
            SetBloomColor(new Color(1f, 0.72f, 0f, 1f));
        }
        #endregion

        private void OnDestroy()
        {
            StopPulse();
            RemoveBloom();
        }
    }

    /// <summary>
    /// Helper component for neon text effects
    /// Creates a glowing neon appearance on text
    /// </summary>
    public class NeonTextEffect : MonoBehaviour
    {
        [Header("Neon Settings")]
        [SerializeField] private Color neonColor = new Color(0f, 0.83f, 1f, 1f);
        [SerializeField] private float neonIntensity = 1f;
        [SerializeField] private bool flicker = false;
        [SerializeField] private float flickerSpeed = 0.1f;

        private TextMeshProUGUI textMesh;
        private BloomEffect bloomEffect;
        private float nextFlickerTime;
        private Coroutine flickerCoroutine;

        private void Awake()
        {
            textMesh = GetComponent<TextMeshProUGUI>();
            bloomEffect = GetComponent<BloomEffect>();
            
            if (bloomEffect == null)
            {
                bloomEffect = gameObject.AddComponent<BloomEffect>();
            }

            bloomEffect.SetBloomColor(neonColor);
            bloomEffect.SetBloomIntensity(neonIntensity);
        }

        private void Start()
        {
            if (flicker)
            {
                StartFlicker();
            }
        }

        private void Update()
        {
            if (flicker && flickerCoroutine == null)
            {
                CheckFlicker();
            }
        }

        private void CheckFlicker()
        {
            if (Time.time >= nextFlickerTime)
            {
                // Random flicker
                if (Random.value < 0.3f)
                {
                    textMesh.enabled = false;
                }
                else
                {
                    textMesh.enabled = true;
                    
                    // Random intensity change
                    float randomIntensity = Random.Range(0.7f, 1.3f) * neonIntensity;
                    bloomEffect.SetBloomIntensity(randomIntensity);
                }

                nextFlickerTime = Time.time + flickerSpeed;
            }
        }

        public void SetNeonColor(Color color)
        {
            neonColor = color;
            bloomEffect.SetBloomColor(color);
        }

        public void SetFlicker(bool enable)
        {
            flicker = enable;
            textMesh.enabled = true;
            
            if (flicker)
            {
                StartFlicker();
            }
            else
            {
                StopFlicker();
            }
        }

        private void StartFlicker()
        {
            if (flickerCoroutine == null)
            {
                flickerCoroutine = StartCoroutine(FlickerCoroutine());
            }
        }

        private void StopFlicker()
        {
            if (flickerCoroutine != null)
            {
                StopCoroutine(flickerCoroutine);
                flickerCoroutine = null;
            }
            textMesh.enabled = true;
            bloomEffect.SetBloomIntensity(neonIntensity);
        }

        private IEnumerator FlickerCoroutine()
        {
            while (flicker)
            {
                // Random on/off intervals
                if (Random.value < 0.3f)
                {
                    textMesh.enabled = false;
                    yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
                }
                else
                {
                    textMesh.enabled = true;
                    float randomIntensity = Random.Range(0.7f, 1.3f) * neonIntensity;
                    bloomEffect.SetBloomIntensity(randomIntensity);
                    yield return new WaitForSeconds(flickerSpeed);
                }
            }
        }

        public void SetFlickerSpeed(float speed)
        {
            flickerSpeed = speed;
        }

        private void OnDestroy()
        {
            StopFlicker();
        }
    }
}