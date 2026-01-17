using System;
using UnityEngine;

public class AbilityController : MonoBehaviour
{
    [Header("Ability Settings")]
    public GameObject shieldVisual;
    public ParticleSystem shieldParticles;
    public AudioSource abilityAudioSource;
    public float abilityCooldown = 5f;
    
    private bool isAbilityReady = true;
    private float lastAbilityTime = -1f;
    private bool isShieldActive = false;
    
    private void Start()
    {
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
        
        if (shieldParticles != null)
        {
            shieldParticles.Stop();
        }
    }
    
    public void OnAbilityInput()
    {
        if (!isAbilityReady) return;
        
        // Check cooldown
        if (Time.time - lastAbilityTime < abilityCooldown) return;
        
        ActivateAbility();
        lastAbilityTime = Time.time;
        isAbilityReady = false;
        
        // Start cooldown timer
        Invoke("ResetAbilityReady", abilityCooldown);
    }
    
    private void ActivateAbility()
    {
        // Activate shield
        isShieldActive = true;
        
        // Play ability effects
        PlayAbilityEffects();
        
        // Shield lasts for 5 seconds (100 ticks at 20Hz)
        Invoke("DeactivateShield", 5f);
    }
    
    private void DeactivateShield()
    {
        isShieldActive = false;
        
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
        
        if (shieldParticles != null && shieldParticles.isPlaying)
        {
            shieldParticles.Stop();
        }
    }
    
    private void PlayAbilityEffects()
    {
        // Play sound
        if (abilityAudioSource != null)
        {
            abilityAudioSource.Play();
        }
        
        // Show shield visual
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true);
        }
        
        // Play shield particles
        if (shieldParticles != null)
        {
            shieldParticles.Play();
        }
    }
    
    public void UpdateShieldState(bool active, int shieldHealth, int maxShieldHealth)
    {
        isShieldActive = active;
        
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(active);
            
            if (active)
            {
                // Update shield size based on health
                float healthRatio = (float)shieldHealth / maxShieldHealth;
                shieldVisual.transform.localScale = Vector3.one * healthRatio;
            }
        }
    }
    
    public bool IsAbilityReady()
    {
        return isAbilityReady;
    }
    
    public float GetAbilityCooldownProgress()
    {
        if (isAbilityReady) return 1f;
        return Mathf.Clamp01((Time.time - lastAbilityTime) / abilityCooldown);
    }
    
    public bool IsShieldActive()
    {
        return isShieldActive;
    }
}