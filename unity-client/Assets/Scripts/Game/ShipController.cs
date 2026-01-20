using System;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    [Header("Ship Settings")]
    public float movementSpeed = 5f;
    public float rotationSpeed = 10f;
    public float interpolationSpeed = 10f;
    
    [Header("References")]
    public Transform thrusterTransform;
    public ParticleSystem thrusterParticles;
    public GameObject shieldVisual;
    
    // Current state
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private int currentHealth = 100;
    private int currentShieldHealth = 0;
    private bool isShieldActive = false;
    private int shieldEndTick = 0;
    private bool isFireReady = true;
    private int fireReadyTick = 0;
    private bool isAbilityReady = true;
    private long lastAbilityTime = 0;
    
    // Player info
    private int playerId = -1;
    private bool isLocalPlayer = false;
    
    private void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        
        if (thrusterParticles != null)
        {
            thrusterParticles.Stop();
        }
        
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
    }
    
    private void Update()
    {
        // Smooth interpolation to target position and rotation
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * interpolationSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * interpolationSpeed);
        
        // Update thruster effects based on movement
        UpdateThrusterEffects();
        
        // Update shield visual
        UpdateShieldVisual();
    }
    
    public void Initialize(int id, bool isLocal)
    {
        playerId = id;
        isLocalPlayer = isLocal;
    }
    
    public void UpdateFromSnapshot(PlayerStateData state)
    {
        if (state == null) return;

        // Update position and rotation
        targetPosition = new Vector3(state.x, 0f, state.y); // Assuming 2D movement on XZ plane
        targetRotation = Quaternion.Euler(0f, state.rotation, 0f);

        // Update health and shield
        currentHealth = state.health;
        currentShieldHealth = state.shieldHealth;
        isShieldActive = state.shieldActive;
        shieldEndTick = state.shieldEndTick;
        isFireReady = state.fireReady;
        fireReadyTick = state.fireReadyTick;
        isAbilityReady = state.abilityReady;
        lastAbilityTime = state.lastAbilityTime;
    }
    
    private void UpdateThrusterEffects()
    {
        if (thrusterParticles == null) return;
        
        // Calculate movement magnitude
        float movementMagnitude = Vector3.Distance(transform.position, targetPosition);
        
        if (movementMagnitude > 0.1f)
        {
            // Moving - show thrusters
            if (!thrusterParticles.isPlaying)
            {
                thrusterParticles.Play();
            }
            
            // Rotate thruster to face movement direction
            if (thrusterTransform != null)
            {
                Vector3 direction = (targetPosition - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    thrusterTransform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }
        else
        {
            // Not moving - stop thrusters
            if (thrusterParticles.isPlaying)
            {
                thrusterParticles.Stop();
            }
        }
    }
    
    private void UpdateShieldVisual()
    {
        if (shieldVisual == null) return;
        
        shieldVisual.SetActive(isShieldActive);
        
        if (isShieldActive)
        {
            // Update shield visual based on health
            float shieldHealthRatio = (float)currentShieldHealth / 50f; // Assuming max shield health is 50
            shieldVisual.transform.localScale = Vector3.one * shieldHealthRatio;
        }
    }
    
    public int GetPlayerId()
    {
        return playerId;
    }
    
    public bool IsLocalPlayer()
    {
        return isLocalPlayer;
    }
    
    public int GetHealth()
    {
        return currentHealth;
    }
    
    public int GetShieldHealth()
    {
        return currentShieldHealth;
    }
    
    public bool IsShieldActive()
    {
        return isShieldActive;
    }
    
    public bool IsFireReady()
    {
        return isFireReady;
    }
    
    public bool IsAbilityReady()
    {
        return isAbilityReady;
    }
}