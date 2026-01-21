using System;
using System.Collections.Generic;

namespace PvpGame.Game
{
    [Serializable]
    public class GameStateData
    {
        public int matchId;
        public PlayerState player1;
        public PlayerState player2;
        public List<BulletState> bullets = new List<BulletState>();
        public int tick;
        public long timestamp;
        public int? winner;
        public string status;
    }

    [Serializable]
    public class PlayerState
    {
        public int id;
        public float x;
        public float y;
        public float rotation;
        public int health;
        public bool abilityReady;
        public long lastAbilityTime;
    }

    [Serializable]
    public class BulletState
    {
        public int id;
        public float x;
        public float y;
        public float rotation;
        public int ownerId;
    }

    [Serializable]
    public class GameInputData
    {
        public float moveX;
        public float moveY;
        public bool fire;
        public bool ability;
        public long timestamp;
    }
}
