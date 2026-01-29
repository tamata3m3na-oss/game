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
            socketClient.OnMatchEnd += OnMatchEnd;

            // Enable input
            if (InputSender.Instance != null)
                InputSender.Instance.EnableInput();
            
            Debug.Log("[GameController] Game initialized, waiting for first snapshot...");
        }

        private void OnSnapshotReceived(GameStateSnapshot snapshot)
        {
            if (snapshot == null) return;

            // Process snapshot through handler
            snapshotHandler.ProcessSnapshot(snapshot);

            // Update ships
            UpdateShips(snapshot);

            // Update bullets
            UpdateBullets(snapshot);
        }

        private void OnMatchEnd(MatchEndEvent result)
        {
            Debug.Log($"[GAME] Match ended. Winner: {result.winnerId}");
            GameManager.Instance.SetMatchResult(result);
            SceneManager.LoadScene("Result");
        }

        private void UpdateShips(GameStateSnapshot snapshot)
        {
            if (snapshot.ships == null) return;

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
            // (Simplification: if not in snapshot, destroy)
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
                    Destroy(view.gameObject);
                    activeShips.Remove(id);
                }
            }
        }

        private void CreateShip(ShipState shipData)
        {
            if (activeShips.ContainsKey(shipData.id)) return;

            GameObject shipObj = Instantiate(shipPrefab);
            shipObj.name = $"Ship_{shipData.id}";

            ShipView shipView = shipObj.GetComponent<ShipView>();
            if (shipView == null) shipView = shipObj.AddComponent<ShipView>();

            // Determine if local player
            // Need UserID from AuthService
            int myId = AuthService.Instance.GetUserId();
            // Assuming shipData.id matches userId string or logic
            // The ticket DTO defines ShipState.id as string. 
            // AuthService UserData.id is int.
            // Assuming string comparison "123" == 123
            
            bool isPlayer = (shipData.id == myId.ToString());
            
            shipView.Initialize(shipData.id, isPlayer);
            shipView.UpdateFromSnapshot(shipData);

            activeShips[shipData.id] = shipView;
        }

        private void UpdateBullets(GameStateSnapshot snapshot)
        {
            if (snapshot.bullets == null)
            {
                foreach (var b in activeBullets.Values) Destroy(b.gameObject);
                activeBullets.Clear();
                return;
            }

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
                    Destroy(view.gameObject);
                    activeBullets.Remove(id);
                }
            }
        }

        private void CreateBullet(BulletState bulletData)
        {
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
            if (socketClient != null)
            {
                socketClient.OnStateSnapshot -= OnSnapshotReceived;
                socketClient.OnMatchEnd -= OnMatchEnd;
            }
        }
    }
}
