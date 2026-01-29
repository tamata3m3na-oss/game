using UnityEngine;
using ShipBattle.Network;

namespace ShipBattle.Gameplay
{
    /// <summary>
    /// Visual representation of a ship in the game.
    /// Updates only from server snapshots - no local logic.
    /// Phase 2 - New implementation.
    /// </summary>
    public class ShipView : MonoBehaviour
    {
        [Header("Visual Components")]
        [SerializeField] private SpriteRenderer shipSprite;
        [SerializeField] private Transform healthBarFill;
        [SerializeField] private Transform shieldBarFill;
        [SerializeField] private GameObject shieldVisual;
        [SerializeField] private ParticleSystem hitEffect;
        [SerializeField] private ParticleSystem fireEffect;

        [Header("Configuration")]
        [SerializeField] private Color playerColor = Color.cyan;
        [SerializeField] private Color opponentColor = Color.red;

        private string shipId;
        private bool isLocalPlayer;
        private float lastHealth = 100f;

        public string ShipId => shipId;
        public bool IsLocalPlayer => isLocalPlayer;

        public void Initialize(string id, bool isPlayer)
        {
            shipId = id;
            isLocalPlayer = isPlayer;

            if (shipSprite != null)
            {
                shipSprite.color = isPlayer ? playerColor : opponentColor;
            }

            Debug.Log($"[ShipView] Initialized - ID: {id}, IsPlayer: {isPlayer}");
        }

        public void UpdateFromSnapshot(ShipData data)
        {
            if (data == null)
            {
                Debug.LogWarning($"[ShipView] Null ship data for {shipId}");
                return;
            }

            // Update position
            if (data.position != null)
            {
                transform.position = new Vector3(data.position.x, data.position.y, 0f);
            }

            // Update rotation
            transform.rotation = Quaternion.Euler(0f, 0f, data.rotation);

            // Update health bar
            UpdateHealthBar(data.health);

            // Update shield
            UpdateShield(data.shield);

            // Check if ship took damage
            if (data.health < lastHealth && hitEffect != null)
            {
                hitEffect.Play();
            }

            lastHealth = data.health;
        }

        private void UpdateHealthBar(float health)
        {
            if (healthBarFill != null)
            {
                float healthPercent = Mathf.Clamp01(health / 100f);
                healthBarFill.localScale = new Vector3(healthPercent, 1f, 1f);
            }
        }

        private void UpdateShield(float shield)
        {
            if (shieldBarFill != null)
            {
                float shieldPercent = Mathf.Clamp01(shield / 100f);
                shieldBarFill.localScale = new Vector3(shieldPercent, 1f, 1f);
            }

            if (shieldVisual != null)
            {
                shieldVisual.SetActive(shield > 0f);
            }
        }

        public void PlayFireEffect()
        {
            if (fireEffect != null)
            {
                fireEffect.Play();
            }
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = new Vector3(position.x, position.y, 0f);
        }

        public void SetRotation(float rotation)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, rotation);
        }

        private void OnValidate()
        {
            // Auto-assign components in editor
            if (shipSprite == null)
            {
                shipSprite = GetComponent<SpriteRenderer>();
            }
        }
    }
}
