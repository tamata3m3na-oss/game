using UnityEngine;
using ShipBattle.Network;

namespace ShipBattle.Gameplay
{
    /// <summary>
    /// Visual representation of a bullet in the game.
    /// Updates only from server snapshots - no local logic.
    /// Auto-destroys when off-screen.
    /// Phase 2 - New implementation.
    /// </summary>
    public class BulletView : MonoBehaviour
    {
        [Header("Visual Components")]
        [SerializeField] private SpriteRenderer bulletSprite;
        [SerializeField] private TrailRenderer trail;

        [Header("Configuration")]
        [SerializeField] private Color playerBulletColor = Color.cyan;
        [SerializeField] private Color opponentBulletColor = Color.red;
        [SerializeField] private float offScreenMargin = 2f;

        private string bulletId;
        private string ownerId;
        private Camera mainCamera;

        public string BulletId => bulletId;
        public string OwnerId => ownerId;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        public void Initialize(string id, string owner, bool isPlayerBullet)
        {
            bulletId = id;
            ownerId = owner;

            if (bulletSprite != null)
            {
                bulletSprite.color = isPlayerBullet ? playerBulletColor : opponentBulletColor;
            }

            if (trail != null)
            {
                Color trailColor = isPlayerBullet ? playerBulletColor : opponentBulletColor;
                trailColor.a = 0.5f;
                trail.startColor = trailColor;
                trail.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
            }
        }

        public void UpdateFromSnapshot(BulletData data)
        {
            if (data == null)
            {
                Debug.LogWarning($"[BulletView] Null bullet data for {bulletId}");
                return;
            }

            // Update position
            if (data.position != null)
            {
                transform.position = new Vector3(data.position.x, data.position.y, 0f);
            }

            // Update rotation based on direction
            if (data.direction != null)
            {
                float angle = Mathf.Atan2(data.direction.y, data.direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            // Check if off-screen and destroy
            CheckOffScreen();
        }

        private void CheckOffScreen()
        {
            if (mainCamera == null)
            {
                return;
            }

            Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

            bool isOffScreen = viewportPos.x < -offScreenMargin || viewportPos.x > 1f + offScreenMargin ||
                               viewportPos.y < -offScreenMargin || viewportPos.y > 1f + offScreenMargin;

            if (isOffScreen)
            {
                Destroy(gameObject);
            }
        }

        private void OnValidate()
        {
            // Auto-assign components in editor
            if (bulletSprite == null)
            {
                bulletSprite = GetComponent<SpriteRenderer>();
            }

            if (trail == null)
            {
                trail = GetComponent<TrailRenderer>();
            }
        }
    }
}
