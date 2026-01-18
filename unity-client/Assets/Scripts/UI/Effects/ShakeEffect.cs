using UnityEngine;
using DG.Tweening;

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

        private Tween shakeTween;
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
            if (shakeTween != null && shakeTween.IsActive())
            {
                shakeTween.Kill();
            }

            shakeTween = transform.DOShakePosition(durationOverride, strengthOverride, vibrato, 90, randomize, fadeOut)
                .OnComplete(() => transform.localPosition = originalPosition);
        }

        public void ShakeRotation(float strengthOverride, float durationOverride)
        {
            if (shakeTween != null && shakeTween.IsActive())
            {
                shakeTween.Kill();
            }

            shakeTween = transform.DOShakeRotation(durationOverride, strengthOverride, vibrato, 90, randomize)
                .OnComplete(() => transform.localRotation = originalRotation);
        }

        public void ShakeScale(float strengthOverride, float durationOverride)
        {
            if (shakeTween != null && shakeTween.IsActive())
            {
                shakeTween.Kill();
            }

            shakeTween = transform.DOShakeScale(durationOverride, strengthOverride, vibrato)
                .OnComplete(() => transform.localScale = originalScale);
        }

        public void StopShake()
        {
            if (shakeTween != null && shakeTween.IsActive())
            {
                shakeTween.Kill();
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
                mainCamera.DOShakePosition(duration, strength, 10, 90, false, true);
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
            if (shakeTween != null && shakeTween.IsActive())
            {
                shakeTween.Kill();
            }
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

        public void Shake(float amount)
        {
            AddTrauma(amount);
        }

        private void Update()
        {
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
        }

        private float GetPerlinNoise(float x, float y)
        {
            return (Mathf.PerlinNoise(x, y) - 0.5f) * 2f;
        }
    }
}
