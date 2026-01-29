using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GameStateSnapshot
{
    public int tick;
    public long timestamp;
    public List<ShipState> ships = new List<ShipState>();
    public List<BulletState> bullets = new List<BulletState>();
}
