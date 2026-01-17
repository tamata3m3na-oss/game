using System.Collections.Generic;
using UnityEngine;

namespace UI.Animations
{
    /// <summary>
    /// Manages all particle systems for visual effects
    /// Object pooling for performance optimization
    /// </summary>
    public class ParticleController : MonoBehaviour
    {
        public static ParticleController Instance { get; private set; }

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
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            InitializePools();
        }

        private void InitializePools()
        {
            CreatePool("background", backgroundParticlePrefab, 30);
            CreatePool("impact", impactParticlePrefab, 50);
            CreatePool("shieldBreak", shieldBreakPrefab, 20);
            CreatePool("explosion", explosionPrefab, 10);
            CreatePool("confetti", confettiPrefab, 30);
            CreatePool("thruster", thrusterParticlePrefab, 10);
        }

        private void CreatePool(string key, ParticleSystem prefab, int size)
        {
            if (prefab == null) return;

            Queue<ParticleSystem> pool = new Queue<ParticleSystem>();
            for (int i = 0; i < size; i++)
            {
                ParticleSystem ps = Instantiate(prefab, transform);
                ps.gameObject.SetActive(false);
                pool.Enqueue(ps);
            }

            particlePools[key] = pool;
        }

        #region Spawn Methods
        public ParticleSystem SpawnParticle(string key, Vector3 position, Quaternion rotation, float lifetime = -1)
        {
            if (!particlePools.ContainsKey(key) || particlePools[key].Count == 0)
            {
                return null;
            }

            ParticleSystem ps = particlePools[key].Dequeue();
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
            ParticleSystem ps = SpawnParticle("impact", position, 0.5f);
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = new Color(1f, 0.5f, 0f, 1f); // Orange spark
            }
        }

        public void SpawnShieldBreakEffect(Vector3 position)
        {
            ParticleSystem ps = SpawnParticle("shieldBreak", position, 0.6f);
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = new Color(0f, 1f, 1f, 1f); // Cyan particles
            }
        }

        public void SpawnExplosionEffect(Vector3 position, float scale = 1f)
        {
            ParticleSystem ps = SpawnParticle("explosion", position, 1f);
            if (ps != null)
            {
                ps.transform.localScale = Vector3.one * scale;
                var main = ps.main;
                main.startColor = new Color(1f, 0.2f, 0f, 1f); // Red-orange
            }
        }

        public void SpawnConfettiEffect(Vector3 position, int burstCount = 50)
        {
            ParticleSystem ps = SpawnParticle("confetti", position, 2f);
            if (ps != null)
            {
                var emission = ps.emission;
                emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, burstCount) });
            }
        }

        public void SpawnThrusterTrail(Vector3 position, Quaternion rotation, float duration = 0.2f)
        {
            ParticleSystem ps = SpawnParticle("thruster", position, rotation, duration);
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = new Color(0f, 0.8f, 1f, 0.8f); // Cyan-blue
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
