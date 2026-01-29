using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameStateSnapshot
{
    public int tick;
    public long timestamp;
    public List<ShipState> ships;
    public List<BulletState> bullets;
}

[Serializable]
public class ShipState
{
    public string id;
    public Vector3 position;
    public float rotation;
    public int health;
    public int shield;
}

[Serializable]
public class BulletState
{
    public string id;
    public Vector3 position;
    public Vector3 direction;
    public string ownerId;
}
