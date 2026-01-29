using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShipBattle.Network
{
    /// <summary>
    /// Handles game state snapshots from the server.
    /// Parses snapshots and manages interpolation data.
    /// Phase 2 - New implementation.
    /// </summary>
    public class SnapshotHandler
    {
        private GameStateSnapshot lastSnapshot;
        private GameStateSnapshot currentSnapshot;
        private float snapshotLerpTime = 0f;
        private const float SNAPSHOT_INTERVAL = 0.05f; // 20Hz = 50ms

        public GameStateSnapshot CurrentSnapshot => currentSnapshot;
        public GameStateSnapshot LastSnapshot => lastSnapshot;

        public event Action<GameStateSnapshot> OnSnapshotReceived;

        public void ProcessSnapshot(GameStateSnapshot snapshot)
        {
            if (snapshot == null)
            {
                Debug.LogWarning("[SnapshotHandler] Received null snapshot");
                return;
            }

            // Store snapshots for interpolation
            lastSnapshot = currentSnapshot;
            currentSnapshot = snapshot;
            snapshotLerpTime = 0f;

            OnSnapshotReceived?.Invoke(snapshot);
        }

        public float GetInterpolationFactor()
        {
            snapshotLerpTime += Time.deltaTime;
            return Mathf.Clamp01(snapshotLerpTime / SNAPSHOT_INTERVAL);
        }

        // Helper methods for accessing data
        public ShipState GetShipState(string shipId)
        {
            if (currentSnapshot?.ships != null)
            {
                foreach (var ship in currentSnapshot.ships)
                {
                    if (ship.id == shipId) return ship;
                }
            }
            return null;
        }

        public List<BulletState> GetAllBullets()
        {
            return currentSnapshot?.bullets ?? new List<BulletState>();
        }

        public void Reset()
        {
            lastSnapshot = null;
            currentSnapshot = null;
            snapshotLerpTime = 0f;
        }
    }
}
