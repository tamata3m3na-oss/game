using UnityEngine;
using System.Collections;

namespace UI.Effects
{
    /// <summary>
    /// Adds shake effects to UI elements
    /// Useful for error feedback, damage indicators, etc.
    /// </summary>
    public class ShakeEffect : MonoBehaviour
    {
        [Header("Shake Settings")]
        [SerializeField] private float strength = 5f;
        [SerializeField] private int vibrato = 10;
        [SerializeField] private float duration = 0.4f;
        [SerializeField] private bool randomize = true;
        [SerializeField] private bool fadeOut = true;

        [Header("Shake Type")]
        [SerializeField] private ShakeType shakeType = ShakeType.Position;

        private Coroutine shakeCoroutine;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Vector3 originalScale;

        private void Awake()
        {
            originalPosition = transform.localPosition;
            originalRotation = transform.localRotation;
            originalScale = transform.localScale;
        }

        public void Shake(float strengthOverride = -1, float durationOverride = -1)
        {
            float actualStrength = strengthOverride > 0 ? strengthOverride : strength;
            float actualDuration = durationOverride > 0 ? durationOverride : duration;

            switch (shakeType)
            {
                case ShakeType.Position:
                    ShakePosition(actualStrength, actualDuration);
                    break;
                case ShakeType.Rotation:
                    ShakeRotation(actualStrength, actualDuration);
                    break;
                case ShakeType.Scale:
                    ShakeScale(actualStrength, actualDuration);
                    break;
            }
        }

        public void ShakePosition(float strengthOverride, float durationOverride)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }

            shakeCoroutine = StartCoroutine(ShakePositionCoroutine(durationOverride, strengthOverride));
        }

        private IEnumerator ShakePositionCoroutine(float duration, float strength)
        {
            float elapsed = 0f;
            Vector3 originalPos = transform.localPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float percentComplete = elapsed / duration;
                
                float currentStrength = fadeOut ? strength * (1f - percentComplete) : strength;
                
                if (randomize)
                {
                    float x = Random.Range(-1f, 1f) * currentStrength;
                    float y = Random.Range(-1f, 1f) * currentStrength;
                    transform.localPosition = originalPos + new Vector3(x, y, 0f);
                }
                else
                {
                    float t = elapsed * vibrato;
                    float x = Mathf.Sin(t) * currentStrength;
                    float y = Mathf.Cos(t * 1.3f) * currentStrength;
                    transform.localPosition = originalPos + new Vector3(x, y, 0f);
                }
                
                yield return null;
            }
            
            transform.localPosition = originalPos;
            shakeCoroutine = null;
        }

        public void ShakeRotation(float strengthOverride, float durationOverride)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }

            shakeCoroutine = StartCoroutine(ShakeRotationCoroutine(durationOverride, strengthOverride));
        }

        private IEnumerator ShakeRotationCoroutine(float duration, float strength)
        {
            float elapsed = 0f;
            Quaternion originalRot = transform.localRotation;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float percentComplete = elapsed / duration;
                
                float currentStrength = fadeOut ? strength * (1f - percentComplete) : strength;
                
                if (randomize)
                {
                    float angle = Random.Range(-1f, 1f) * currentStrength;
                    transform.localRotation = originalRot * Quaternion.Euler(0f, 0f, angle);
                }
                else
                {
                    float t = elapsed * vibrato;
                    float angle = Mathf.Sin(t) * currentStrength;
                    transform.localRotation = originalRot * Quaternion.Euler(0f, 0f, angle);
                }
                
                yield return null;
            }
            
            transform.localRotation = originalRot;
            shakeCoroutine = null;
        }

        public void ShakeScale(float strengthOverride, float durationOverride)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }

            shakeCoroutine = StartCoroutine(ShakeScaleCoroutine(durationOverride, strengthOverride));
        }

        private IEnumerator ShakeScaleCoroutine(float duration, float strength)
        {
            float elapsed = 0f;
            Vector3 originalScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float percentComplete = elapsed / duration;
                
                float currentStrength = fadeOut ? strength * (1f - percentComplete) : strength;
                
                if (randomize)
                {
                    float scaleFactor = 1f + Random.Range(-1f, 1f) * currentStrength * 0.1f;
                    transform.localScale = originalScale * scaleFactor;
                }
                else
                {
                    float t = elapsed * vibrato;
                    float scaleFactor = 1f + Mathf.Sin(t) * currentStrength * 0.1f;
                    transform.localScale = originalScale * scaleFactor;
                }
                
                yield return null;
            }
            
            transform.localScale = originalScale;
            shakeCoroutine = null;
        }

        public void StopShake()
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
            }

            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
            transform.localScale = originalScale;
        }

        public void SetShakeType(ShakeType type)
        {
            shakeType = type;
        }

        #region Preset Shake Patterns
        public void ErrorShake()
        {
            Shake(8f, 0.4f);
        }

        public void DamageShake()
        {
            Shake(15f, 0.2f);
        }

        public void SubtleShake()
        {
            Shake(3f, 0.3f);
        }

        public void ExplosiveShake()
        {
            Shake(20f, 0.5f);
        }
        #endregion

        #region Screen Shake (Camera)
        public static void ShakeCamera(float strength = 1f, float duration = 0.3f)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.gameObject.AddComponent<CameraShake>().Shake(strength, duration);
            }
        }

        public static void ShakeCameraImpact()
        {
            ShakeCamera(0.5f, 0.15f);
        }

        public static void ShakeCameraExplosion()
        {
            ShakeCamera(1.5f, 0.4f);
        }
        #endregion

        private void OnDestroy()
        {
            StopShake();
        }
    }

    public enum ShakeType
    {
        Position,
        Rotation,
        Scale
    }

    /// <summary>
    /// Helper component for camera shake effects
    /// Attach to Camera object for easy access to camera shake
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        [Header("Shake Settings")]
        [SerializeField] private float trauma = 0f;
        [SerializeField] private float traumaDecay = 1.5f;
        [SerializeField] private float traumaMagnitude = 0.5f;

        [Header("Camera Shake Settings")]
        [SerializeField] private float maxAngle = 5f;
        [SerializeField] private float maxOffset = 0.5f;

        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private float seed;

        private void Awake()
        {
            originalPosition = transform.localPosition;
            originalRotation = transform.localRotation;
            seed = Random.value * 100f;
        }

        public void AddTrauma(float amount)
        {
            trauma = Mathf.Clamp01(trauma + amount);
        }

        public void Shake(float strength, float duration)
        {
            AddTrauma(strength);
            
            // Start shake coroutine
            StartCoroutine(ShakeCoroutine(duration));
        }

        private IEnumerator ShakeCoroutine(float duration)
        {
            float elapsed = 0f;
            
            while (elapsed < duration && trauma > 0f)
            {
                elapsed += Time.deltaTime;
                
                if (trauma > 0f)
                {
                    float shake = trauma * trauma;
                    
                    // Calculate shake offsets
                    float angle = maxAngle * shake * GetPerlinNoise(Time.time * 25f, seed);
                    float offsetX = maxOffset * shake * GetPerlinNoise(Time.time * 25f, seed + 1f);
                    float offsetY = maxOffset * shake * GetPerlinNoise(Time.time * 25f, seed + 2f);

                    // Apply shake
                    transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
                    transform.localRotation = originalRotation * Quaternion.Euler(0f, 0f, angle);

                    // Decay trauma
                    trauma = Mathf.Clamp01(trauma - traumaDecay * Time.deltaTime);
                }
                else
                {
                    // Return to original position
                    transform.localPosition = originalPosition;
                    transform.localRotation = originalRotation;
                }
                
                yield return null;
            }
            
            // Final reset
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
            
            // Destroy this component when done
            Destroy(this);
        }

        private float GetPerlinNoise(float x, float y)
        {
            return (Mathf.PerlinNoise(x, y) - 0.5f) * 2f;
        }
    }
}