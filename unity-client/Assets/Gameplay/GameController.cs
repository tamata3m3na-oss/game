using System.Collections.Generic;
using UnityEngine;
using ShipBattle.Network;
using ShipBattle.Core;

namespace ShipBattle.Gameplay
{
    /// <summary>
    /// Main game controller for the match scene.
    /// Manages ships, bullets, and game state based on server snapshots.
    /// Phase 2 - New implementation.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject shipPrefab;
        [SerializeField] private GameObject bulletPrefab;

        [Header("Spawn Configuration")]
        [SerializeField] private Vector2 playerSpawnPosition = new Vector2(-5f, 0f);
        [SerializeField] private Vector2 opponentSpawnPosition = new Vector2(5f, 0f);

        private SocketClient socketClient;
        private SnapshotHandler snapshotHandler;

        private Dictionary<string, ShipView> activeShips = new Dictionary<string, ShipView>();
        private Dictionary<string, BulletView> activeBullets = new Dictionary<string, BulletView>();

        private string localPlayerId;
        private bool isInitialized;

        private void Start()
        {
            socketClient = GameManager.Instance.SocketClient;
            snapshotHandler = new SnapshotHandler();

            if (socketClient == null)
            {
                Debug.LogError("[GameController] SocketClient not found!");
                return;
            }

            // Subscribe to snapshots
            socketClient.OnStateSnapshot += OnSnapshotReceived;

            // Get local player ID (this should come from authentication)
            // For now, we'll determine it from the first snapshot
            
            Debug.Log("[GameController] Game initialized, waiting for first snapshot...");
        }

        private void OnSnapshotReceived(GameSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            // Process snapshot through handler
            snapshotHandler.ProcessSnapshot(snapshot);

            // Initialize on first snapshot
            if (!isInitialized && snapshot.ships != null && snapshot.ships.Length > 0)
            {
                InitializeGame(snapshot);
                isInitialized = true;
            }

            // Update ships
            UpdateShips(snapshot);

            // Update bullets
            UpdateBullets(snapshot);
        }

        private void InitializeGame(GameSnapshot snapshot)
        {
            Debug.Log("[GameController] Initializing game with first snapshot");

            // Create ships based on snapshot
            if (snapshot.ships != null)
            {
                for (int i = 0; i < snapshot.ships.Length; i++)
                {
                    ShipData shipData = snapshot.ships[i];
                    bool isPlayer = i == 0; // First ship is player (convention)

                    CreateShip(shipData, isPlayer);
                }
            }
        }

        private void CreateShip(ShipData shipData, bool isPlayer)
        {
            if (activeShips.ContainsKey(shipData.id))
            {
                return;
            }

            GameObject shipObj = Instantiate(shipPrefab);
            shipObj.name = $"Ship_{shipData.id}";

            ShipView shipView = shipObj.GetComponent<ShipView>();
            if (shipView == null)
            {
                shipView = shipObj.AddComponent<ShipView>();
            }

            shipView.Initialize(shipData.id, isPlayer);
            shipView.UpdateFromSnapshot(shipData);

            activeShips[shipData.id] = shipView;

            Debug.Log($"[GameController] Created ship - ID: {shipData.id}, IsPlayer: {isPlayer}");
        }

        private void UpdateShips(GameSnapshot snapshot)
        {
            if (snapshot.ships == null)
            {
                return;
            }

            // Update existing ships
            foreach (ShipData shipData in snapshot.ships)
            {
                if (activeShips.TryGetValue(shipData.id, out ShipView shipView))
                {
                    shipView.UpdateFromSnapshot(shipData);
                }
                else
                {
                    // Ship not found, create it
                    bool isPlayer = activeShips.Count == 0; // First ship is player
                    CreateShip(shipData, isPlayer);
                }
            }

            // Remove ships not in snapshot (destroyed)
            List<string> shipsToRemove = new List<string>();
            foreach (var kvp in activeShips)
            {
                bool foundInSnapshot = false;
                foreach (ShipData shipData in snapshot.ships)
                {
                    if (shipData.id == kvp.Key)
                    {
                        foundInSnapshot = true;
                        break;
                    }
                }

                if (!foundInSnapshot)
                {
                    shipsToRemove.Add(kvp.Key);
                }
            }

            foreach (string shipId in shipsToRemove)
            {
                if (activeShips.TryGetValue(shipId, out ShipView shipView))
                {
                    Destroy(shipView.gameObject);
                    activeShips.Remove(shipId);
                    Debug.Log($"[GameController] Removed ship: {shipId}");
                }
            }
        }

        private void UpdateBullets(GameSnapshot snapshot)
        {
            if (snapshot.bullets == null)
            {
                // Remove all bullets if none in snapshot
                foreach (var kvp in activeBullets)
                {
                    Destroy(kvp.Value.gameObject);
                }
                activeBullets.Clear();
                return;
            }

            // Track bullets in current snapshot
            HashSet<string> currentBulletIds = new HashSet<string>();

            // Update or create bullets
            foreach (BulletData bulletData in snapshot.bullets)
            {
                currentBulletIds.Add(bulletData.id);

                if (activeBullets.TryGetValue(bulletData.id, out BulletView bulletView))
                {
                    // Update existing bullet
                    bulletView.UpdateFromSnapshot(bulletData);
                }
                else
                {
                    // Create new bullet
                    CreateBullet(bulletData);
                }
            }

            // Remove bullets not in snapshot
            List<string> bulletsToRemove = new List<string>();
            foreach (var kvp in activeBullets)
            {
                if (!currentBulletIds.Contains(kvp.Key))
                {
                    bulletsToRemove.Add(kvp.Key);
                }
            }

            foreach (string bulletId in bulletsToRemove)
            {
                if (activeBullets.TryGetValue(bulletId, out BulletView bulletView))
                {
                    Destroy(bulletView.gameObject);
                    activeBullets.Remove(bulletId);
                }
            }
        }

        private void CreateBullet(BulletData bulletData)
        {
            GameObject bulletObj = Instantiate(bulletPrefab);
            bulletObj.name = $"Bullet_{bulletData.id}";

            BulletView bulletView = bulletObj.GetComponent<BulletView>();
            if (bulletView == null)
            {
                bulletView = bulletObj.AddComponent<BulletView>();
            }

            // Determine if this is a player bullet
            bool isPlayerBullet = activeShips.ContainsKey(bulletData.ownerId) && 
                                  activeShips[bulletData.ownerId].IsLocalPlayer;

            bulletView.Initialize(bulletData.id, bulletData.ownerId, isPlayerBullet);
            bulletView.UpdateFromSnapshot(bulletData);

            activeBullets[bulletData.id] = bulletView;
        }

        private void OnDestroy()
        {
            if (socketClient != null)
            {
                socketClient.OnStateSnapshot -= OnSnapshotReceived;
            }
        }
    }
}
