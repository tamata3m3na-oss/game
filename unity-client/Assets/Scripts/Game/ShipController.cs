using UnityEngine;
using PvpGame.Config;
using PvpGame.Utils;

namespace PvpGame.Game
{
    public class ShipController : MonoBehaviour
    {
        [Header("References")]
        public SpriteRenderer spriteRenderer;
        public HealthDisplay healthDisplay;

        [Header("Settings")]
        public Color playerColor = Color.blue;
        public Color opponentColor = Color.red;

        private int playerId;
        private Vector3 targetPosition;
        private float targetRotation;
        private int currentHealth;
        private GameConfig config;

        private void Awake()
        {
            config = GameConfig.Instance;
            targetPosition = transform.position;
            targetRotation = transform.eulerAngles.z;
        }

        public void Initialize(int id, bool isLocalPlayer)
        {
            playerId = id;
            spriteRenderer.color = isLocalPlayer ? playerColor : opponentColor;
            Logger.LogGame($"Ship initialized for player {id} (Local: {isLocalPlayer})");
        }

        public void UpdateFromState(PlayerState state)
        {
            if (state == null || state.id != playerId) return;

            targetPosition = new Vector3(state.x, state.y, 0f);
            targetRotation = state.rotation;
            currentHealth = state.health;

            if (healthDisplay != null)
            {
                healthDisplay.SetHealth(currentHealth, config.maxHealth);
            }
        }

        private void Update()
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                Time.deltaTime * config.interpolationSpeed
            );

            float currentZ = transform.eulerAngles.z;
            float newZ = Mathf.LerpAngle(currentZ, targetRotation, Time.deltaTime * config.interpolationSpeed);
            transform.eulerAngles = new Vector3(0, 0, newZ);
        }

        public int GetPlayerId()
        {
            return playerId;
        }

        public int GetHealth()
        {
            return currentHealth;
        }
    }
}
