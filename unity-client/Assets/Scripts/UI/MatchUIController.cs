using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchUIController : MonoBehaviour
{
    [Header("Health Bars")]
    public Image playerHealthBar;
    public Image opponentHealthBar;
    public Image playerShieldBar;
    public Image opponentShieldBar;

    [Header("Cooldown Indicators")]
    public Image fireCooldownIndicator;
    public Image abilityCooldownIndicator;
    public TextMeshProUGUI fireCooldownText;
    public TextMeshProUGUI abilityCooldownText;

    [Header("Player Info")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI opponentNameText;
    public TextMeshProUGUI timerText;

    [Header("Debug Info")]
    public TextMeshProUGUI fpsText;
    public TextMeshProUGUI pingText;

    private GameStateManager gameStateManager;
    private ShipController playerShip;
    private ShipController opponentShip;
    private float matchStartTime;

    private void Start()
    {
        gameStateManager = FindObjectOfType<GameStateManager>();
        if (gameStateManager != null)
        {
            playerShip = gameStateManager.GetPlayerShip();
            opponentShip = gameStateManager.GetOpponentShip();
        }
        else
        {
            Debug.LogWarning("[MatchUIController] No GameStateManager found in scene.");
        }

        if (playerNameText != null && AuthManager.Instance != null)
        {
            playerNameText.text = AuthManager.Instance.GetUsername();
        }

        if (opponentNameText != null)
        {
            opponentNameText.text = NetworkEventManager.GetInstance(false)?.LastMatchStart?.opponent?.username ?? "Opponent";
        }

        matchStartTime = Time.time;
    }

    private void Update()
    {
        UpdateHealthBars();
        UpdateCooldownIndicators();
        UpdateTimer();
        UpdateDebugInfo();
    }

    private void UpdateHealthBars()
    {
        if (playerShip != null && playerHealthBar != null)
        {
            float healthRatio = (float)playerShip.GetHealth() / 100f;
            playerHealthBar.fillAmount = healthRatio;
            playerHealthBar.color = Color.Lerp(Color.red, Color.green, healthRatio);
        }

        if (opponentShip != null && opponentHealthBar != null)
        {
            float healthRatio = (float)opponentShip.GetHealth() / 100f;
            opponentHealthBar.fillAmount = healthRatio;
            opponentHealthBar.color = Color.Lerp(Color.red, Color.green, healthRatio);
        }

        UpdateShieldBar(playerShip, playerShieldBar);
        UpdateShieldBar(opponentShip, opponentShieldBar);
    }

    private void UpdateShieldBar(ShipController ship, Image shieldBar)
    {
        if (ship == null || shieldBar == null) return;

        if (ship.IsShieldActive())
        {
            shieldBar.gameObject.SetActive(true);
            float shieldRatio = (float)ship.GetShieldHealth() / 50f;
            shieldBar.fillAmount = shieldRatio;
            shieldBar.color = Color.blue;
        }
        else
        {
            shieldBar.gameObject.SetActive(false);
        }
    }

    private void UpdateCooldownIndicators()
    {
        WeaponController weapon = playerShip?.GetComponent<WeaponController>();
        AbilityController ability = playerShip?.GetComponent<AbilityController>();

        if (weapon != null && fireCooldownIndicator != null)
        {
            float cooldownProgress = weapon.GetFireCooldownProgress();
            fireCooldownIndicator.fillAmount = 1f - cooldownProgress;

            if (fireCooldownText != null)
            {
                fireCooldownText.text = weapon.IsFireReady()
                    ? "READY"
                    : ((1f - cooldownProgress) * weapon.GetFireCooldown()).ToString("F1") + "s";
            }
        }

        if (ability != null && abilityCooldownIndicator != null)
        {
            float cooldownProgress = ability.GetAbilityCooldownProgress();
            abilityCooldownIndicator.fillAmount = 1f - cooldownProgress;

            if (abilityCooldownText != null)
            {
                abilityCooldownText.text = ability.IsAbilityReady()
                    ? "READY"
                    : ((1f - cooldownProgress) * ability.GetAbilityCooldown()).ToString("F1") + "s";
            }
        }
    }

    private void UpdateTimer()
    {
        if (timerText != null)
        {
            float matchTime = Time.time - matchStartTime;
            TimeSpan timeSpan = TimeSpan.FromSeconds(matchTime);
            timerText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }
    }

    private void UpdateDebugInfo()
    {
        if (gameStateManager == null) return;

        if (fpsText != null)
        {
            fpsText.text = "FPS: " + gameStateManager.GetCurrentFPS();
        }

        if (pingText != null)
        {
            pingText.text = "Ping: " + (gameStateManager.GetSnapshotDelay() * 1000).ToString("F0") + "ms";
        }
    }

    public void SetOpponentName(string name)
    {
        if (opponentNameText != null)
        {
            opponentNameText.text = name;
        }
    }
}
