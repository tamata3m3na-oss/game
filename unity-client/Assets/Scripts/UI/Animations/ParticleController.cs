using System.Collections.Generic;
using UnityEngine;

namespace UI.Animations
{
    /// <summary>
    /// Manages all particle systems for visual effects
    /// Object pooling for performance optimization
    /// </summary>
    [DefaultExecutionOrder(-150)]
    public class ParticleController : MonoBehaviour
    {
        public static ParticleController Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogWarning("[ParticleController] Instance is null. Make sure ParticleController is properly initialized.");
                }
                return instance;
            }
            private set
            {
                instance = value;
            }
        }
        
        private static ParticleController instance;

        [Header("Particle Prefabs")]
        [SerializeField] private ParticleSystem backgroundParticlePrefab;
        [SerializeField] private ParticleSystem impactParticlePrefab;
        [SerializeField] private ParticleSystem shieldBreakPrefab;
        [SerializeField] private ParticleSystem explosionPrefab;
        [SerializeField] private ParticleSystem confettiPrefab;
        [SerializeField] private ParticleSystem thrusterParticlePrefab;

        [Header("Object Pool Settings")]
        [SerializeField] private int poolSize = 50;

        private Dictionary<string, Queue<ParticleSystem>> particlePools = new Dictionary<string, Queue<ParticleSystem>>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[ParticleController] Duplicate instance detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize pools with safety checks
            if (!InitializePools())
            {
                Debug.LogError("[ParticleController] Failed to initialize particle pools. Some effects may not work.");
            }
        }

        private bool InitializePools()
        {
            bool allPoolsInitialized = true;
            
            allPoolsInitialized &= CreatePool("background", backgroundParticlePrefab, 30);
            allPoolsInitialized &= CreatePool("impact", impactParticlePrefab, 50);
            allPoolsInitialized &= CreatePool("shieldBreak", shieldBreakPrefab, 20);
            allPoolsInitialized &= CreatePool("explosion", explosionPrefab, 10);
            allPoolsInitialized &= CreatePool("confetti", confettiPrefab, 30);
            allPoolsInitialized &= CreatePool("thruster", thrusterParticlePrefab, 10);
            
            return allPoolsInitialized;
        }

        private bool CreatePool(string key, ParticleSystem prefab, int size)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"[ParticleController] Prefab for pool '{key}' is null. Pool will not be created.");
                return false;
            }

            try
            {
                Queue<ParticleSystem> pool = new Queue<ParticleSystem>();
                for (int i = 0; i < size; i++)
                {
                    ParticleSystem ps = Instantiate(prefab, transform);
                    if (ps != null)
                    {
                        ps.gameObject.SetActive(false);
                        pool.Enqueue(ps);
                    }
                    else
                    {
                        Debug.LogWarning($"[ParticleController] Failed to instantiate particle system for pool '{key}' at index {i}.");
                    }
                }

                particlePools[key] = pool;
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ParticleController] Exception creating pool '{key}': {ex.Message}");
                return false;
            }
        }

        #region Spawn Methods
        public ParticleSystem SpawnParticle(string key, Vector3 position, Quaternion rotation, float lifetime = -1)
        {
            if (Instance == null)
            {
                Debug.LogWarning("[ParticleController] Cannot spawn particle - Instance is null.");
                return null;
            }

            if (!particlePools.ContainsKey(key) || particlePools[key].Count == 0)
            {
                Debug.LogWarning($"[ParticleController] No available particles in pool '{key}' or pool doesn't exist.");
                return null;
            }

            try
            {
                ParticleSystem ps = particlePools[key].Dequeue();
                if (ps == null)
                {
                    Debug.LogWarning($"[ParticleController] Dequeued null particle from pool '{key}'.");
                    return null;
                }

                ps.transform.position = position;
                ps.transform.rotation = rotation;
                ps.gameObject.SetActive(true);
                ps.Play();

                if (lifetime > 0)
                {
                    StartCoroutine(ReturnToPoolAfterDelay(key, ps, lifetime));
                }

                return ps;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ParticleController] Exception spawning particle '{key}': {ex.Message}");
                return null;
            }
        }

        public ParticleSystem SpawnParticle(string key, Vector3 position, float lifetime = -1)
        {
            return SpawnParticle(key, position, Quaternion.identity, lifetime);
        }

        private System.Collections.IEnumerator ReturnToPoolAfterDelay(string key, ParticleSystem ps, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnToPool(key, ps);
        }

        private void ReturnToPool(string key, ParticleSystem ps)
        {
            if (ps == null) return;

            ps.Stop();
            ps.gameObject.SetActive(false);
            
            if (particlePools.ContainsKey(key))
            {
                particlePools[key].Enqueue(ps);
            }
        }
        #endregion

        #region Specific Effects
        public void SpawnImpactEffect(Vector3 position)
        {
            if (Instance == null)
            {
                Debug.LogWarning("[ParticleController] Cannot spawn impact effect - Instance is null.");
                return;
            }

            ParticleSystem ps = SpawnParticle("impact", position, 0.5f);
            if (ps != null)
            {
                try
                {
                    var main = ps.main;
                    main.startColor = new Color(1f, 0.5f, 0f, 1f); // Orange spark
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[ParticleController] Exception setting impact effect properties: {ex.Message}");
                }
            }
        }

        public void SpawnShieldBreakEffect(Vector3 position)
        {
            if (Instance == null)
            {
                Debug.LogWarning("[ParticleController] Cannot spawn shield break effect - Instance is null.");
                return;
            }

            ParticleSystem ps = SpawnParticle("shieldBreak", position, 0.6f);
            if (ps != null)
            {
                try
                {
                    var main = ps.main;
                    main.startColor = new Color(0f, 1f, 1f, 1f); // Cyan particles
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[ParticleController] Exception setting shield break effect properties: {ex.Message}");
                }
            }
        }

        public void SpawnExplosionEffect(Vector3 position, float scale = 1f)
        {
            if (Instance == null)
            {
                Debug.LogWarning("[ParticleController] Cannot spawn explosion effect - Instance is null.");
                return;
            }

            ParticleSystem ps = SpawnParticle("explosion", position, 1f);
            if (ps != null)
            {
                try
                {
                    ps.transform.localScale = Vector3.one * scale;
                    var main = ps.main;
                    main.startColor = new Color(1f, 0.2f, 0f, 1f); // Red-orange
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[ParticleController] Exception setting explosion effect properties: {ex.Message}");
                }
            }
        }

        public void SpawnConfettiEffect(Vector3 position, int burstCount = 50)
        {
            if (Instance == null)
            {
                Debug.LogWarning("[ParticleController] Cannot spawn confetti effect - Instance is null.");
                return;
            }

            ParticleSystem ps = SpawnParticle("confetti", position, 2f);
            if (ps != null)
            {
                try
                {
                    var emission = ps.emission;
                    emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, burstCount) });
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[ParticleController] Exception setting confetti effect properties: {ex.Message}");
                }
            }
        }

        public void SpawnThrusterTrail(Vector3 position, Quaternion rotation, float duration = 0.2f)
        {
            if (Instance == null)
            {
                Debug.LogWarning("[ParticleController] Cannot spawn thruster trail - Instance is null.");
                return;
            }

            ParticleSystem ps = SpawnParticle("thruster", position, rotation, duration);
            if (ps != null)
            {
                try
                {
                    var main = ps.main;
                    main.startColor = new Color(0f, 0.8f, 1f, 0.8f); // Cyan-blue
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[ParticleController] Exception setting thruster trail properties: {ex.Message}");
                }
            }
        }
        #endregion

        #region Background Particles
        private List<ParticleSystem> activeBackgroundParticles = new List<ParticleSystem>();

        public void StartBackgroundParticles()
        {
            StopBackgroundParticles();

            for (int i = 0; i < 20; i++)
            {
                Vector3 randomPos = new Vector3(
                    Random.Range(-10f, 10f),
                    Random.Range(-6f, 6f),
                    0f
                );

                ParticleSystem ps = SpawnParticle("background", randomPos);
                if (ps != null)
                {
                    activeBackgroundParticles.Add(ps);
                    
                    // Randomize particle properties
                    var main = ps.main;
                    main.startLifetime = 5f + Random.Range(0f, 3f);
                    main.startSpeed = 1f + Random.Range(0f, 0.5f);
                    
                    // Set color randomly between cyan and magenta
                    if (Random.value > 0.5f)
                    {
                        main.startColor = new Color(0f, 0.83f, 1f, 0.6f); // Cyan
                    }
                    else
                    {
                        main.startColor = new Color(1f, 0f, 0.43f, 0.6f); // Magenta
                    }
                }
            }
        }

        public void StopBackgroundParticles()
        {
            foreach (ParticleSystem ps in activeBackgroundParticles)
            {
                if (ps != null)
                {
                    ps.Stop();
                    ReturnToPool("background", ps);
                }
            }
            activeBackgroundParticles.Clear();
        }
        #endregion

        #region Combat Effects
        public void SpawnDamageNumber(Vector3 position, int damage, bool isCritical = false)
        {
            GameObject damageObj = new GameObject("DamageNumber");
            damageObj.transform.position = position;
            
            TextMesh textMesh = damageObj.AddComponent<TextMesh>();
            textMesh.text = damage.ToString();
            textMesh.fontSize = isCritical ? 24 : 16;
            textMesh.color = isCritical ? Color.yellow : Color.white;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;

            // Add floating animation
            damageObj.AddComponent<DamageNumberAnimator>();

            // Auto-destroy after animation
            Destroy(damageObj, 1f);
        }
        #endregion

        #region Cleanup
        private void OnDestroy()
        {
            StopAllCoroutines();
            StopBackgroundParticles();
        }
        #endregion
    }

    /// <summary>
    /// Simple animator for floating damage numbers
    /// </summary>
    public class DamageNumberAnimator : MonoBehaviour
    {
        private float lifetime = 0.8f;
        private float elapsed = 0f;
        private Vector3 velocity;
        private float startScale = 1.2f;

        private void Start()
        {
            velocity = Vector3.up * 2f + Vector3.right * Random.Range(-0.5f, 0.5f);
            transform.localScale = Vector3.one * startScale;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;

            if (elapsed >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            // Move upward
            transform.position += velocity * Time.deltaTime;
            velocity *= 0.98f; // Decelerate

            // Scale down
            float progress = elapsed / lifetime;
            float scale = Mathf.Lerp(startScale, 0.8f, progress);
            transform.localScale = Vector3.one * scale;

            // Fade out
            TextMesh textMesh = GetComponent<TextMesh>();
            if (textMesh != null)
            {
                Color color = textMesh.color;
                color.a = 1f - progress;
                textMesh.color = color;
            }
        }
    }
}
