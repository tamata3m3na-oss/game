using System;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Settings")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float fireCooldown = 0.5f;
    public AudioSource fireAudioSource;
    public ParticleSystem muzzleFlash;
    
    [Header("Bullet Pooling")]
    public int poolSize = 50;
    public Transform bulletPoolContainer;
    
    private ObjectPool<Bullet> bulletPool;
    private float lastFireTime = -1f;
    private bool isFireReady = true;
    
    private void Start()
    {
        // Initialize bullet pool
        bulletPool = new ObjectPool<Bullet>(CreateBullet, OnGetBullet, OnReleaseBullet, OnDestroyBullet, true, poolSize, poolSize * 2);
        
        // Pre-warm pool
        for (int i = 0; i < poolSize; i++)
        {
            var bullet = bulletPool.Get();
            bulletPool.Release(bullet);
        }
    }
    
    private Bullet CreateBullet()
    {
        GameObject bulletObj = Instantiate(bulletPrefab, bulletPoolContainer);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        bullet.SetPool(bulletPool);
        return bullet;
    }
    
    private void OnGetBullet(Bullet bullet)
    {
        bullet.gameObject.SetActive(true);
    }
    
    private void OnReleaseBullet(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
    }
    
    private void OnDestroyBullet(Bullet bullet)
    {
        Destroy(bullet.gameObject);
    }
    
    public void OnFireInput()
    {
        if (!isFireReady) return;
        
        // Check cooldown
        if (Time.time - lastFireTime < fireCooldown) return;
        
        FireWeapon();
        lastFireTime = Time.time;
        isFireReady = false;
        
        // Start cooldown timer
        Invoke("ResetFireReady", fireCooldown);
    }
    
    private void FireWeapon()
    {
        if (firePoint == null) return;
        
        // Play fire effects
        PlayFireEffects();
        
        // Get bullet from pool
        var bullet = bulletPool.Get();
        if (bullet != null)
        {
            bullet.transform.position = firePoint.position;
            bullet.transform.rotation = firePoint.rotation;
            bullet.Fire();
        }
    }
    
    private void PlayFireEffects()
    {
        // Play sound
        if (fireAudioSource != null)
        {
            fireAudioSource.Play();
        }
        
        // Play muzzle flash
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }
    
    private void ResetFireReady()
    {
        isFireReady = true;
    }
    
    public bool IsFireReady()
    {
        return isFireReady;
    }
    
    public float GetFireCooldownProgress()
    {
        if (isFireReady) return 1f;
        return Mathf.Clamp01((Time.time - lastFireTime) / fireCooldown);
    }
}