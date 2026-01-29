using System.Collections.Generic;
using UnityEngine;
using ShipBattle.Network;
using ShipBattle.Core;
using UnityEngine.SceneManagement;

namespace ShipBattle.Gameplay
{
    public class GameController : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject shipPrefab;
        [SerializeField] private GameObject bulletPrefab;

        private SocketClient socketClient;
        private SnapshotHandler snapshotHandler;

        private Dictionary<string, ShipView> activeShips = new Dictionary<string, ShipView>();
        private Dictionary<string, BulletView> activeBullets = new Dictionary<string, BulletView>();

        private bool isInitialized;

        private void Awake()
        {
            Debug.Log("[GameController] Awake");
        }

        private void Start()
        {
            Debug.Log("[GameController] Start - Initializing game scene");
            
            if (GameManager.Instance == null)
            {
                Debug.LogError("[GameController] GameManager instance is null!");
                return;
            }
            
            socketClient = GameManager.Instance.SocketClient;
            snapshotHandler = new SnapshotHandler();

            if (socketClient == null)
            {
                Debug.LogError("[GameController] SocketClient not found!");
                return;
            }

            Debug.Log("[GameController] Subscribing to SocketClient events");
            // Subscribe to snapshots
            socketClient.OnStateSnapshot += OnSnapshotReceived;
            socketClient.OnMatchEnd += OnMatchEnd;

            // Enable input
            if (InputSender.Instance != null)
            {
                InputSender.Instance.EnableInput();
                Debug.Log("[GameController] Input enabled");
            }
            else
            {
                Debug.LogWarning("[GameController] InputSender instance not found");
            }
            
            Debug.Log("[GameController] Game initialized, waiting for first snapshot...");
        }

        private void OnSnapshotReceived(GameStateSnapshot snapshot)
        {
            if (snapshot == null) 
            {
                Debug.LogWarning("[GameController] Received null snapshot");
                return;
            }

            Debug.Log($"[GameController] Snapshot received: Ships={snapshot.ships?.Count ?? 0}, Bullets={snapshot.bullets?.Count ?? 0}");

            // Process snapshot through handler
            snapshotHandler.ProcessSnapshot(snapshot);

            // Update ships
            UpdateShips(snapshot);

            // Update bullets
            UpdateBullets(snapshot);
        }

        private void OnMatchEnd(MatchEndEvent result)
        {
            Debug.Log($"[GameController] Match ended. Winner: {result.winnerId}, Rating change: {result.ratingChange}");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetMatchResult(result);
            }
            
            Debug.Log("[GameController] Loading Result scene...");
            SceneManager.LoadScene("Result");
        }

        private void UpdateShips(GameStateSnapshot snapshot)
        {
            if (snapshot.ships == null) return;

            Debug.Log($"[GameController] Updating {snapshot.ships.Count} ships");

            // Update existing ships
            foreach (ShipState shipData in snapshot.ships)
            {
                if (activeShips.TryGetValue(shipData.id, out ShipView shipView))
                {
                    shipView.UpdateFromSnapshot(shipData);
                }
                else
                {
                    CreateShip(shipData);
                }
            }

            // Remove destroyed ships
            List<string> toRemove = new List<string>();
            foreach (var kvp in activeShips)
            {
                bool found = false;
                foreach (var s in snapshot.ships)
                {
                    if (s.id == kvp.Key) { found = true; break; }
                }
                if (!found) toRemove.Add(kvp.Key);
            }

            foreach (var id in toRemove)
            {
                if (activeShips.TryGetValue(id, out var view))
                {
                    Debug.Log($"[GameController] Destroying ship: {id}");
                    Destroy(view.gameObject);
                    activeShips.Remove(id);
                }
            }
        }

        private void CreateShip(ShipState shipData)
        {
            if (activeShips.ContainsKey(shipData.id)) return;

            Debug.Log($"[GameController] Creating ship: {shipData.id}");

            GameObject shipObj = Instantiate(shipPrefab);
            shipObj.name = $"Ship_{shipData.id}";

            ShipView shipView = shipObj.GetComponent<ShipView>();
            if (shipView == null) shipView = shipObj.AddComponent<ShipView>();

            // Determine if local player
            int myId = AuthService.Instance.GetUserId();
            bool isPlayer = (shipData.id == myId.ToString());
            
            Debug.Log($"[GameController] Ship {shipData.id} isPlayer: {isPlayer} (myId: {myId})");
            
            shipView.Initialize(shipData.id, isPlayer);
            shipView.UpdateFromSnapshot(shipData);

            activeShips[shipData.id] = shipView;
        }

        private void UpdateBullets(GameStateSnapshot snapshot)
        {
            if (snapshot.bullets == null)
            {
                Debug.Log("[GameController] No bullets in snapshot, clearing all bullets");
                foreach (var b in activeBullets.Values) Destroy(b.gameObject);
                activeBullets.Clear();
                return;
            }

            Debug.Log($"[GameController] Updating {snapshot.bullets.Count} bullets");
            HashSet<string> currentIds = new HashSet<string>();

            foreach (BulletState bulletData in snapshot.bullets)
            {
                currentIds.Add(bulletData.id);

                if (activeBullets.TryGetValue(bulletData.id, out BulletView bulletView))
                {
                    bulletView.UpdateFromSnapshot(bulletData);
                }
                else
                {
                    CreateBullet(bulletData);
                }
            }

            List<string> toRemove = new List<string>();
            foreach (var kvp in activeBullets)
            {
                if (!currentIds.Contains(kvp.Key)) toRemove.Add(kvp.Key);
            }

            foreach (var id in toRemove)
            {
                if (activeBullets.TryGetValue(id, out var view))
                {
                    Debug.Log($"[GameController] Destroying bullet: {id}");
                    Destroy(view.gameObject);
                    activeBullets.Remove(id);
                }
            }
        }

        private void CreateBullet(BulletState bulletData)
        {
            Debug.Log($"[GameController] Creating bullet: {bulletData.id}");
            
            GameObject bulletObj = Instantiate(bulletPrefab);
            bulletObj.name = $"Bullet_{bulletData.id}";

            BulletView bulletView = bulletObj.GetComponent<BulletView>();
            if (bulletView == null) bulletView = bulletObj.AddComponent<BulletView>();

            // Check owner
            int myId = AuthService.Instance.GetUserId();
            bool isPlayerBullet = (bulletData.ownerId == myId.ToString());

            bulletView.Initialize(bulletData.id, bulletData.ownerId, isPlayerBullet);
            bulletView.UpdateFromSnapshot(bulletData);

            activeBullets[bulletData.id] = bulletView;
        }

        private void OnDestroy()
        {
            Debug.Log("[GameController] OnDestroy - Unsubscribing from events");
            if (socketClient != null)
            {
                socketClient.OnStateSnapshot -= OnSnapshotReceived;
                socketClient.OnMatchEnd -= OnMatchEnd;
            }
            
            // Disable input
            if (InputSender.Instance != null)
            {
                InputSender.Instance.DisableInput();
                Debug.Log("[GameController] Input disabled");
            }
        }
    }
}
