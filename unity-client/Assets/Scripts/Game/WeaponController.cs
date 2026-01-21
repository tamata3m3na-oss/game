using UnityEngine;
using PvpGame.Utils;

namespace PvpGame.Game
{
    public class WeaponController : MonoBehaviour
    {
        [Header("Visual Effects")]
        public GameObject fireEffectPrefab;
        public Transform firePoint;

        private float lastFireTime;
        private float fireRateLimit = 0.1f;

        public bool TryFire()
        {
            if (Time.time - lastFireTime < fireRateLimit)
            {
                return false;
            }

            lastFireTime = Time.time;
            PlayFireEffect();
            AppLogger.LogGame("Weapon fired");
            return true;
        }

        private void PlayFireEffect()
        {
            if (fireEffectPrefab != null && firePoint != null)
            {
                GameObject effect = Instantiate(fireEffectPrefab, firePoint.position, firePoint.rotation);
                Destroy(effect, 0.5f);
            }
        }
    }
}
