using System;

namespace ShipBattle.Network.Models
{
    [Serializable]
    public class MatchEndEvent
    {
        public string matchId;
        public string winnerId;
        public int ratingChange;
    }
}
