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
        private GameSnapshot lastSnapshot;
        private GameSnapshot currentSnapshot;
        private float snapshotLerpTime = 0f;
        private const float SNAPSHOT_INTERVAL = 0.05f; // 20Hz = 50ms

        public GameSnapshot CurrentSnapshot => currentSnapshot;
        public GameSnapshot LastSnapshot => lastSnapshot;

        public event Action<GameSnapshot> OnSnapshotReceived;

        public void ProcessSnapshot(GameSnapshot snapshot)
        {
            if (snapshot == null)
            {
                Debug.LogWarning("[SnapshotHandler] Received null snapshot");
                return;
            }

            // Validate snapshot data
            if (!ValidateSnapshot(snapshot))
            {
                Debug.LogWarning("[SnapshotHandler] Invalid snapshot data");
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

        public Vector2 GetInterpolatedPosition(string entityId)
        {
            if (lastSnapshot == null || currentSnapshot == null)
            {
                return Vector2.zero;
            }

            float t = GetInterpolationFactor();

            // Try to find ship
            Vector2Data lastPos = GetEntityPosition(lastSnapshot, entityId);
            Vector2Data currentPos = GetEntityPosition(currentSnapshot, entityId);

            if (lastPos != null && currentPos != null)
            {
                return Vector2.Lerp(
                    new Vector2(lastPos.x, lastPos.y),
                    new Vector2(currentPos.x, currentPos.y),
                    t
                );
            }

            // If not found in last snapshot, just use current
            if (currentPos != null)
            {
                return new Vector2(currentPos.x, currentPos.y);
            }

            return Vector2.zero;
        }

        public float GetInterpolatedRotation(string entityId)
        {
            if (lastSnapshot == null || currentSnapshot == null)
            {
                return 0f;
            }

            float t = GetInterpolationFactor();

            float lastRot = GetShipRotation(lastSnapshot, entityId);
            float currentRot = GetShipRotation(currentSnapshot, entityId);

            return Mathf.LerpAngle(lastRot, currentRot, t);
        }

        private Vector2Data GetEntityPosition(GameSnapshot snapshot, string entityId)
        {
            // Check ships
            if (snapshot.ships != null)
            {
                foreach (var ship in snapshot.ships)
                {
                    if (ship.id == entityId)
                    {
                        return ship.position;
                    }
                }
            }

            // Check bullets
            if (snapshot.bullets != null)
            {
                foreach (var bullet in snapshot.bullets)
                {
                    if (bullet.id == entityId)
                    {
                        return bullet.position;
                    }
                }
            }

            return null;
        }

        private float GetShipRotation(GameSnapshot snapshot, string entityId)
        {
            if (snapshot.ships != null)
            {
                foreach (var ship in snapshot.ships)
                {
                    if (ship.id == entityId)
                    {
                        return ship.rotation;
                    }
                }
            }

            return 0f;
        }

        public ShipData GetShipData(string shipId)
        {
            if (currentSnapshot?.ships != null)
            {
                foreach (var ship in currentSnapshot.ships)
                {
                    if (ship.id == shipId)
                    {
                        return ship;
                    }
                }
            }

            return null;
        }

        public BulletData[] GetAllBullets()
        {
            return currentSnapshot?.bullets ?? new BulletData[0];
        }

        private bool ValidateSnapshot(GameSnapshot snapshot)
        {
            if (snapshot.tick < 0)
            {
                return false;
            }

            if (snapshot.timestamp <= 0)
            {
                return false;
            }

            // Ships and bullets can be null or empty, that's valid

            return true;
        }

        public void Reset()
        {
            lastSnapshot = null;
            currentSnapshot = null;
            snapshotLerpTime = 0f;
        }
    }
}
