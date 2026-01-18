using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UI.Animations;
using UI.Effects;

namespace UI.Scenes
{
    /// <summary>
    /// Enhanced Game Scene with premium combat UI
    /// Includes health bars, cooldowns, damage indicators, and visual feedback
    /// </summary>
    public class GameSceneUI : MonoBehaviour
    {
        [Header("Health Bars")]
        public Image playerHealthBar;
        public Image opponentHealthBar;
        public Image playerShieldBar;
        public Image opponentShieldBar;
        public Image playerHealthBarFill;
        public Image opponentHealthBarFill;
        public Image playerShieldBarFill;
        public Image opponentShieldBarFill;
        
        [Header("Cooldown Indicators")]
        public Image fireCooldownIndicator;
        public Image abilityCooldownIndicator;
        public TextMeshProUGUI fireCooldownText;
        public TextMeshProUGUI abilityCooldownText;
        public Transform fireCooldownTransform;
        public Transform abilityCooldownTransform;
        
        [Header("Player Info")]
        public TextMeshProUGUI playerNameText;
        public TextMeshProUGUI opponentNameText;
        public TextMeshProUGUI timerText;
        public Transform opponentInfoPanel;
        
        [Header("Status Indicators")]
        public TextMeshProUGUI shieldStatusText;
        public TextMeshProUGUI abilityStatusText;
        
        [Header("Feedback UI")]
        public Image damageFlashOverlay;
        public Image vignetteOverlay;
        public Transform hitMarker;
        
        [Header("Debug Info")]
        public TextMeshProUGUI fpsText;
        public TextMeshProUGUI pingText;
        
        [Header("Visual Settings")]
        [SerializeField] private Color healthHighColor = new Color(0f, 1f, 0.53f, 1f); // Green
        [SerializeField] private Color healthMedColor = new Color(1f, 0.92f, 0f, 1f); // Yellow
        [SerializeField] private Color healthLowColor = new Color(1f, 0.27f, 0.27f, 1f); // Red
        [SerializeField] private Color shieldColor = new Color(0f, 0.83f, 1f, 1f); // Cyan
        [SerializeField] private Color abilityReadyColor = new Color(1f, 0f, 0.43f, 1f); // Magenta
        
        private ShipController playerShip;
        private ShipController opponentShip;
        private float matchStartTime;
        private float lastFireCooldown = 0f;
        private float lastAbilityCooldown = 0f;

        private void Start()
        {
            // Initialize managers
            InitializeManagers();
            
            // Get references to ships
            playerShip = GameStateManager.Instance.GetPlayerShip();
            opponentShip = GameStateManager.Instance.GetOpponentShip();
            
            // Set player names
            playerNameText.text = AuthManager.Instance.GetUsername();
            opponentNameText.text = "Opponent";
            
            matchStartTime = Time.time;
            
            // Play entrance animations
            PlayEntranceAnimations();
            
            // Apply visual effects
            ApplyVisualEffects();
            
            // Hide hit marker initially
            if (hitMarker != null)
            {
                hitMarker.gameObject.SetActive(false);
            }
        }

        private void InitializeManagers()
        {
            if (AnimationController.Instance == null)
            {
                GameObject animControllerObj = new GameObject("AnimationController");
                animControllerObj.AddComponent<AnimationController>();
            }

            if (ParticleController.Instance == null)
            {
                GameObject particleControllerObj = new GameObject("ParticleController");
                particleControllerObj.AddComponent<ParticleController>();
            }

            if (TransitionManager.Instance == null)
            {
                GameObject transitionManagerObj = new GameObject("TransitionManager");
                transitionManagerObj.AddComponent<TransitionManager>();
            }
        }

        private void PlayEntranceAnimations()
        {
            // Fade in HUD elements
            CanvasGroup hudCanvas = GetComponent<CanvasGroup>();
            if (hudCanvas == null)
            {
                hudCanvas = gameObject.AddComponent<CanvasGroup>();
            }
            AnimationController.Instance.FadeIn(hudCanvas, 0.3f);
            
            // Staggered fade in for HUD elements
            Transform[] hudElements = new Transform[]
            {
                playerHealthBar?.transform,
                opponentHealthBar?.transform,
                fireCooldownIndicator?.transform,
                abilityCooldownIndicator?.transform,
                timerText?.transform
            };
            
            AnimationController.Instance.StaggeredFadeIn(GetComponentsInChildren<CanvasGroup>(), 0.05f, 0.2f);
            
            // Opponent name glow animation
            if (opponentNameText != null)
            {
                StartCoroutine(OpponentNameGlowAnimation());
            }
        }

        private IEnumerator OpponentNameGlowAnimation()
        {
            while (opponentNameText != null)
            {
                // Glow animation
                opponentNameText.color = new Color(0f, 0.83f, 1f, 1f);
                
                yield return new WaitForSeconds(1f);
                
                opponentNameText.color = Color.white;
                
                yield return new WaitForSeconds(1f);
            }
        }

        private void ApplyVisualEffects()
        {
            // Apply glow to health bars
            ApplyGlowEffect(playerHealthBarFill, healthHighColor);
            ApplyGlowEffect(opponentHealthBarFill, healthHighColor);
            ApplyGlowEffect(playerShieldBarFill, shieldColor);
            ApplyGlowEffect(opponentShieldBarFill, shieldColor);
            
            // Apply glow to cooldown indicators
            ApplyGlowEffect(fireCooldownIndicator, healthHighColor);
            ApplyGlowEffect(abilityCooldownIndicator, abilityReadyColor);
            
            // Apply bloom to status texts
            if (shieldStatusText != null)
            {
                BloomEffect bloom = shieldStatusText.GetComponent<BloomEffect>();
                if (bloom == null)
                {
                    bloom = shieldStatusText.gameObject.AddComponent<BloomEffect>();
                }
                bloom.SetCyanBloom();
            }
            
            if (abilityStatusText != null)
            {
                BloomEffect bloom = abilityStatusText.GetComponent<BloomEffect>();
                if (bloom == null)
                {
                    bloom = abilityStatusText.gameObject.AddComponent<BloomEffect>();
                }
                bloom.SetMagentaBloom();
            }
            
            // Setup vignette
            if (vignetteOverlay != null)
            {
                vignetteOverlay.color = new Color(0f, 0f, 0f, 0f);
            }
        }

        private void ApplyGlowEffect(Image image, Color color)
        {
            if (image == null) return;
            
            GlowEffect glow = image.GetComponent<GlowEffect>();
            if (glow == null)
            {
                glow = image.gameObject.AddComponent<GlowEffect>();
            }
            glow.SetGlowColor(color);
            glow.SetGlowMode(GlowMode.Static);
        }

        private void Update()
        {
            UpdateHealthBars();
            UpdateCooldownIndicators();
            UpdateTimer();
            UpdateDebugInfo();
            UpdateOpponentInfoPanel();
        }

        private void UpdateHealthBars()
        {
            if (playerShip != null && playerHealthBarFill != null)
            {
                float healthRatio = (float)playerShip.GetHealth() / 100f;
                
                // Smooth animation
                AnimationController.Instance.AnimateHealthBar(playerHealthBarFill, playerHealthBarFill.fillAmount, healthRatio, 0.2f);
                
                // Update color based on health
                playerHealthBarFill.color = GetHealthColor(healthRatio);
                
                // Flash effect on damage
                if (playerShip.GetHealth() < 25f)
                {
                    // Critical health - red vignette
                    UpdateVignette(0.3f);
                }
                else
                {
                    UpdateVignette(0f);
                }
            }
            
            if (opponentShip != null && opponentHealthBarFill != null)
            {
                float healthRatio = (float)opponentShip.GetHealth() / 100f;
                AnimationController.Instance.AnimateHealthBar(opponentHealthBarFill, opponentHealthBarFill.fillAmount, healthRatio, 0.2f);
                opponentHealthBarFill.color = GetHealthColor(healthRatio);
            }
            
            // Update shield bars
            UpdateShieldBar(playerShip, playerShieldBar, playerShieldBarFill);
            UpdateShieldBar(opponentShip, opponentShieldBar, opponentShieldBarFill);
        }

        private Color GetHealthColor(float healthRatio)
        {
            if (healthRatio > 0.6f)
            {
                return healthHighColor;
            }
            else if (healthRatio > 0.25f)
            {
                return healthMedColor;
            }
            else
            {
                return healthLowColor;
            }
        }

        private void UpdateShieldBar(ShipController ship, Image shieldBar, Image shieldBarFill)
        {
            if (ship == null || shieldBar == null) return;
            
            if (ship.IsShieldActive())
            {
                shieldBar.gameObject.SetActive(true);
                float shieldRatio = (float)ship.GetShieldHealth() / 50f;
                
                // Smooth animation
                AnimationController.Instance.AnimateHealthBar(shieldBarFill, shieldBarFill.fillAmount, shieldRatio, 0.2f);
                
                shieldBarFill.color = shieldColor;
                
                // Show shield status text
                if (shieldStatusText != null)
                {
                    shieldStatusText.text = "SHIELD ACTIVE";
                    shieldStatusText.gameObject.SetActive(true);
                }
            }
            else
            {
                shieldBar.gameObject.SetActive(false);
                
                if (shieldStatusText != null)
                {
                    shieldStatusText.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateVignette(float intensity)
        {
            if (vignetteOverlay != null)
            {
                Color vignetteColor = Color.black;
                vignetteColor.a = intensity;
                vignetteOverlay.color = vignetteColor;
            }
        }

        private void UpdateCooldownIndicators()
        {
            // Get weapon and ability controllers from player ship
            WeaponController weapon = playerShip?.GetComponent<WeaponController>();
            AbilityController ability = playerShip?.GetComponent<AbilityController>();
            
            if (weapon != null && fireCooldownIndicator != null)
            {
                float cooldownProgress = weapon.GetFireCooldownProgress();
                
                // Smooth animation
                AnimationController.Instance.AnimateHealthBar(fireCooldownIndicator, 1f - cooldownProgress, 1f - cooldownProgress, 0.05f);
                
                if (fireCooldownText != null)
                {
                    if (weapon.IsFireReady())
                    {
                        fireCooldownText.text = "READY";
                        fireCooldownText.color = healthHighColor;
                        
                        // Pulse animation when ready
                        if (lastFireCooldown < 1f)
                        {
                            AnimationController.Instance.Pulse(fireCooldownTransform, 1.1f, 0.3f);
                            
                            if (fireCooldownIndicator != null)
                            {
                                fireCooldownIndicator.color = healthHighColor;
                            }
                        }
                    }
                    else
                    {
                        float cooldownTime = (1f - cooldownProgress) * weapon.GetFireCooldown();
                        fireCooldownText.text = cooldownTime.ToString("F1") + "s";
                        fireCooldownText.color = Color.gray;
                        
                        if (fireCooldownIndicator != null)
                        {
                            fireCooldownIndicator.color = Color.gray;
                        }
                    }
                }
                
                lastFireCooldown = cooldownProgress;
            }
            
            if (ability != null && abilityCooldownIndicator != null)
            {
                float cooldownProgress = ability.GetAbilityCooldownProgress();
                
                // Smooth animation
                AnimationController.Instance.AnimateHealthBar(abilityCooldownIndicator, 1f - cooldownProgress, 1f - cooldownProgress, 0.05f);
                
                if (abilityCooldownText != null)
                {
                    if (ability.IsAbilityReady())
                    {
                        abilityCooldownText.text = "READY";
                        abilityCooldownText.color = abilityReadyColor;
                        
                        // Pulse animation when ready
                        if (lastAbilityCooldown < 1f)
                        {
                            AnimationController.Instance.Pulse(abilityCooldownTransform, 1.1f, 0.3f);
                            
                            // Flash effect
                            if (abilityCooldownIndicator != null)
                            {
                                abilityCooldownIndicator.color = abilityReadyColor;
                            }
                            
                            // Show status text
                            if (abilityStatusText != null)
                            {
                                abilityStatusText.text = "ABILITY READY";
                                abilityStatusText.gameObject.SetActive(true);
                            }
                        }
                    }
                    else
                    {
                        float cooldownTime = (1f - cooldownProgress) * ability.GetAbilityCooldown();
                        abilityCooldownText.text = cooldownTime.ToString("F1") + "s";
                        abilityCooldownText.color = Color.gray;
                        
                        if (abilityCooldownIndicator != null)
                        {
                            abilityCooldownIndicator.color = Color.gray;
                        }
                        
                        if (abilityStatusText != null)
                        {
                            abilityStatusText.gameObject.SetActive(false);
                        }
                    }
                }
                
                lastAbilityCooldown = cooldownProgress;
            }
        }

        private void UpdateTimer()
        {
            if (timerText == null) return;
            
            float matchTime = Time.time - matchStartTime;
            TimeSpan timeSpan = TimeSpan.FromSeconds(matchTime);
            timerText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
            
            // Color transition based on time (example: after 2 minutes, change to yellow)
            if (matchTime > 120f)
            {
                timerText.color = healthMedColor;
            }
            
            // Pulse effect in last 30 seconds
            if (matchTime > 150f && matchTime < 180f)
            {
                timerText.transform.localScale = Vector3.one * (1f + 0.1f * Mathf.Sin(Time.time * 5f));
            }
        }

        private void UpdateOpponentInfoPanel()
        {
            if (opponentInfoPanel == null || opponentShip == null) return;
            
            // Smooth follow animation with lag
            Vector3 targetPosition = opponentShip.transform.position;
            Vector3 currentPos = opponentInfoPanel.position;
            
            // Convert world position to screen position
            Vector3 screenPos = Camera.main.WorldToScreenPoint(targetPosition);
            
            // Smooth interpolation
            opponentInfoPanel.position = Vector3.Lerp(currentPos, screenPos, 5f * Time.deltaTime);
        }

        private void UpdateDebugInfo()
        {
            if (fpsText != null)
            {
                int fps = GameStateManager.Instance.GetCurrentFPS();
                fpsText.text = "FPS: " + fps;
                
                // Color code FPS
                if (fps >= 55)
                {
                    fpsText.color = healthHighColor;
                }
                else if (fps >= 30)
                {
                    fpsText.color = healthMedColor;
                }
                else
                {
                    fpsText.color = healthLowColor;
                }
            }
            
            if (pingText != null)
            {
                float ping = GameStateManager.Instance.GetSnapshotDelay() * 1000;
                pingText.text = "Ping: " + ping.ToString("F0") + "ms";
                
                // Color code ping
                if (ping < 50f)
                {
                    pingText.color = healthHighColor;
                }
                else if (ping < 100f)
                {
                    pingText.color = healthMedColor;
                }
                else
                {
                    pingText.color = healthLowColor;
                }
            }
        }

        #region Combat Feedback Methods
        public void OnPlayerHit(int damage, bool isCritical = false)
        {
            // Damage flash
            if (damageFlashOverlay != null)
            {
                AnimationController.Instance.DamageFlash(damageFlashOverlay, 0.1f);
            }
            
            // Screen shake
            ShakeEffect.ShakeCamera(0.5f, 0.15f);
            
            // Hit marker
            ShowHitMarker();
            
            // Damage number
            if (playerShip != null)
            {
                Vector3 position = playerShip.transform.position;
                ParticleController.Instance?.SpawnDamageNumber(position, damage, isCritical);
            }
            
            // Vignette effect
            UpdateVignette(0.2f);
            StartCoroutine(FadeVignette(0.2f, 0f, 0.5f));
        }

        public void OnPlayerHitEnemy(int damage, bool isCritical = false)
        {
            // Damage number at enemy position
            if (opponentShip != null)
            {
                Vector3 position = opponentShip.transform.position;
                ParticleController.Instance?.SpawnDamageNumber(position, damage, isCritical);
            }
            
            // Impact particles
            if (opponentShip != null)
            {
                ParticleController.Instance?.SpawnImpactEffect(opponentShip.transform.position);
            }
        }

        public void OnShieldActivated()
        {
            // Shield visual effect
            if (playerShip != null)
            {
                ParticleController.Instance?.SpawnShieldBreakEffect(playerShip.transform.position);
            }
            
            // Pulse effect on shield bar
            if (playerShieldBar != null)
            {
                AnimationController.Instance.Pulse(playerShieldBar.transform, 1.1f, 0.2f);
            }
        }

        public void OnShieldBroken()
        {
            // Shield break particles
            if (playerShip != null)
            {
                ParticleController.Instance?.SpawnShieldBreakEffect(playerShip.transform.position);
            }
            
            // Screen shake
            ShakeEffect.ShakeCamera(0.8f, 0.2f);
        }

        public void OnAbilityUsed()
        {
            // Ability visual effect
            if (playerShip != null)
            {
                StartCoroutine(AbilityVisualCoroutine(playerShip.transform.position));
            }
        }

        private IEnumerator AbilityVisualCoroutine(Vector3 position)
        {
            // Expanding shield visual
            GameObject shieldVisual = new GameObject("AbilityVisual");
            shieldVisual.transform.position = position;
            
            // Add circle sprite
            SpriteRenderer sr = shieldVisual.AddComponent<SpriteRenderer>();
            // Would need a circle sprite reference here
            
            // Animate scale up and fade out
            float duration = 0.3f;
            float elapsed = 0f;
            Vector3 startScale = Vector3.one;
            Vector3 targetScale = Vector3.one * 3f;
            Color startColor = sr.color;
            Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseOutQuad(t);
                
                shieldVisual.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                sr.color = Color.Lerp(startColor, targetColor, t);
                
                yield return null;
            }
            
            Destroy(shieldVisual);
        }

        private void ShowHitMarker()
        {
            if (hitMarker == null) return;
            
            StartCoroutine(HitMarkerAnimation());
        }

        private IEnumerator HitMarkerAnimation()
        {
            if (hitMarker == null) yield break;
            
            hitMarker.gameObject.SetActive(true);
            hitMarker.localScale = Vector3.one * 1.2f;
            
            // Scale down animation
            float elapsed = 0f;
            float duration = 0.1f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseOutQuad(t);
                hitMarker.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one * 0.8f, t);
                yield return null;
            }
            
            // Scale up animation
            elapsed = 0f;
            duration = 0.05f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseInQuad(t);
                hitMarker.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, t);
                yield return null;
            }
            
            // Fade out animation
            CanvasGroup canvasGroup = hitMarker.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = hitMarker.gameObject.AddComponent<CanvasGroup>();
            }
            
            elapsed = 0f;
            duration = 0.05f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseInQuad(t);
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }
            
            // Hide hit marker
            hitMarker.gameObject.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        private IEnumerator FadeVignette(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float alpha = Mathf.Lerp(from, to, t);
                UpdateVignette(alpha);
                yield return null;
            }
            UpdateVignette(to);
        }
        #endregion

        public void SetOpponentName(string name)
        {
            if (opponentNameText != null)
            {
                opponentNameText.text = name;
                
                // Glow animation
                AnimationController.Instance.Pulse(opponentNameText.transform, 1.1f, 0.3f);
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

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
