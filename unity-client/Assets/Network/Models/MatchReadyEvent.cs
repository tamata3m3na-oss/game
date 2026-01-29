using System;

[Serializable]
public class MatchReadyEvent
{
    public string matchId;
    public string opponentId;
    public string opponentUsername;
    public int opponentRating;
}
