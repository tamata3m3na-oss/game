using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 8f;
    public float lifetime = 5f;
    public int damage = 10;
    
    private ObjectPool<Bullet> pool;
    private float spawnTime;
    private int shooterId = -1;
    
    private void OnEnable()
    {
        spawnTime = Time.time;
    }
    
    private void Update()
    {
        // Move bullet forward
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        
        // Check lifetime
        if (Time.time - spawnTime > lifetime)
        {
            ReturnToPool();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if hit a ship
        ShipController ship = other.GetComponent<ShipController>();
        if (ship != null && ship.GetPlayerId() != shooterId)
        {
            // Hit opponent ship
            // In a real game, this would be handled by the server
            // For visual purposes only
            Debug.Log("Bullet hit ship " + ship.GetPlayerId());
            ReturnToPool();
        }
        else if (!other.CompareTag("Player") && !other.CompareTag("Bullet"))
        {
            // Hit something else (wall, etc.)
            ReturnToPool();
        }
    }
    
    public void Fire()
    {
        spawnTime = Time.time;
        // shooterId would be set by the weapon controller
    }
    
    public void SetShooterId(int id)
    {
        shooterId = id;
    }
    
    public void SetPool(ObjectPool<Bullet> bulletPool)
    {
        pool = bulletPool;
    }
    
    public void ReturnToPool()
    {
        if (pool != null)
        {
            pool.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}