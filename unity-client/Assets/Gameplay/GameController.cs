using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ShipBattle.Network;
using ShipBattle.Core;
using UnityEngine.SceneManagement;
using TMPro;

namespace ShipBattle.Gameplay
{
    public class GameController : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject shipPrefab;
        [SerializeField] private GameObject bulletPrefab;

        [Header("UI References")]
        [SerializeField] private Slider playerHealthBar;
        [SerializeField] private Slider opponentHealthBar;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private MatchTimer matchTimer;
        [SerializeField] private InputSender inputSender;

        [Header("Ship References")]
        [SerializeField] private ShipView playerShipView;
        [SerializeField] private ShipView opponentShipView;

        private SocketClient socketClient;
        private SnapshotHandler snapshotHandler;

        private Dictionary<string, ShipView> activeShips = new Dictionary<string, ShipView>();
        private Dictionary<string, BulletView> activeBullets = new Dictionary<string, BulletView>();

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

            // Start timer
            if (matchTimer != null) matchTimer.StartTimer();

            // Subscribe to snapshots
            socketClient.OnStateSnapshot += OnSnapshotReceived;
            socketClient.OnMatchEnd += OnMatchEnd;

            // Enable input
            if (inputSender != null)
            {
                inputSender.EnableInput();
                Debug.Log("[GameController] Input enabled");
            }
            else
            {
                // Try to find it if not assigned
                inputSender = FindObjectOfType<InputSender>();
                if (inputSender != null) inputSender.EnableInput();
            }
            
            Debug.Log("[GameController] Game initialized");
        }

        private void Update()
        {
            // Update timer if needed, though usually handled by MatchTimer
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
            
            // Update UI
            UpdateUI(snapshot);
        }

        private void UpdateUI(GameStateSnapshot snapshot)
        {
            if (snapshot.ships == null) return;

            int myId = AuthService.Instance.GetUserId();

            foreach (var shipData in snapshot.ships)
            {
                bool isPlayer = (shipData.id == myId.ToString());
                if (isPlayer)
                {
                    if (playerHealthBar != null) playerHealthBar.value = shipData.health / 100f;
                }
                else
                {
                    if (opponentHealthBar != null) opponentHealthBar.value = shipData.health / 100f;
                }
            }
            
            // Timer update (if snapshot has time)
            // if (timerText != null) timerText.text = snapshot.remainingTime.ToString("F0");
        }

        private void OnMatchEnd(MatchEndEvent result)
        {
            Debug.Log($"[GameController] Match ended. Winner: {result.winnerId}");
            
            if (matchTimer != null) matchTimer.StopTimer();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetMatchResult(result);
            }
            
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
                Destroy(activeShips[id].gameObject);
                activeShips.Remove(id);
            }
        }

        private void CreateShip(ShipState shipData)
        {
            if (shipPrefab == null) return;

            GameObject shipObj = Instantiate(shipPrefab);
            shipObj.name = $"Ship_{shipData.id}";

            ShipView shipView = shipObj.GetComponent<ShipView>();
            if (shipView == null) shipView = shipObj.AddComponent<ShipView>();

            int myId = AuthService.Instance.GetUserId();
            bool isPlayer = (shipData.id == myId.ToString());
            
            shipView.Initialize(shipData.id, isPlayer);
            shipView.UpdateFromSnapshot(shipData);

            activeShips[shipData.id] = shipView;

            // Assign to references if they are the player or opponent
            if (isPlayer) playerShipView = shipView;
            else opponentShipView = shipView;
        }

        private void UpdateBullets(GameStateSnapshot snapshot)
        {
            if (snapshot.bullets == null) return;

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
                Destroy(activeBullets[id].gameObject);
                activeBullets.Remove(id);
            }
        }

        private void CreateBullet(BulletState bulletData)
        {
            if (bulletPrefab == null) return;
            
            GameObject bulletObj = Instantiate(bulletPrefab);
            BulletView bulletView = bulletObj.GetComponent<BulletView>();
            if (bulletView == null) bulletView = bulletObj.AddComponent<BulletView>();

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
            if (inputSender != null) inputSender.DisableInput();
        }
    }
}
