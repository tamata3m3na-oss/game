using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PvpGame.Config;
using PvpGame.Utils;

namespace PvpGame.Game
{
    public class AbilityController : MonoBehaviour
    {
        [Header("UI References")]
        public Image cooldownImage;
        public TextMeshProUGUI cooldownText;
        public GameObject abilityEffectPrefab;

        [Header("Settings")]
        public Color readyColor = Color.green;
        public Color cooldownColor = Color.gray;

        private float lastAbilityTime;
        private float cooldownDuration;
        private bool isReady = true;
        private GameConfig config;

        private void Awake()
        {
            config = GameConfig.Instance;
            cooldownDuration = config.abilityCooldown;
        }

        private void Update()
        {
            UpdateCooldownUI();
        }

        public bool TryUseAbility()
        {
            if (!isReady)
            {
                AppLogger.LogWarning("Ability on cooldown");
                return false;
            }

            lastAbilityTime = Time.time;
            isReady = false;
            PlayAbilityEffect();
            AppLogger.LogGame("Ability used");
            return true;
        }

        public void UpdateAbilityState(bool ready)
        {
            isReady = ready;
            if (ready)
            {
                lastAbilityTime = 0f;
            }
        }

        private void UpdateCooldownUI()
        {
            if (isReady)
            {
                if (cooldownImage != null)
                {
                    cooldownImage.fillAmount = 0f;
                    cooldownImage.color = readyColor;
                }

                if (cooldownText != null)
                {
                    cooldownText.text = "READY";
                }
            }
            else
            {
                float elapsed = Time.time - lastAbilityTime;
                float remaining = Mathf.Max(0f, cooldownDuration - elapsed);
                float fillAmount = remaining / cooldownDuration;

                if (cooldownImage != null)
                {
                    cooldownImage.fillAmount = fillAmount;
                    cooldownImage.color = cooldownColor;
                }

                if (cooldownText != null)
                {
                    cooldownText.text = remaining.ToString("F1") + "s";
                }

                if (remaining <= 0f)
                {
                    isReady = true;
                }
            }
        }

        private void PlayAbilityEffect()
        {
            if (abilityEffectPrefab != null)
            {
                GameObject effect = Instantiate(abilityEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 1f);
            }
        }

        public bool IsReady()
        {
            return isReady;
        }
    }
}
