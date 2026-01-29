using System;

namespace ShipBattle.Network.Models
{
    [Serializable]
    public class MatchReadyEvent
    {
        public string matchId;
        public string opponentId;
        public string opponentUsername;
        public int opponentRating;
    }
}
