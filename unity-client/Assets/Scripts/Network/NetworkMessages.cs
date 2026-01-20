using System;

[Serializable]
public class QueueStatusData
{
    public int position;
    public int estimatedWait;
}

[Serializable]
public class OpponentData
{
    public int id;
    public string username;
    public int rating;
}

[Serializable]
public class MatchFoundData
{
    public int matchId;
    public OpponentData opponent;
}

[Serializable]
public class MatchStartData
{
    public int matchId;
    public OpponentData opponent;
    public string color;
}

[Serializable]
public class GameEndData
{
    public int matchId;
    public int winner;
    public NetworkGameState finalState;
}

[Serializable]
public class MatchReadyData
{
    public int matchId;
}

[Serializable]
public class GameInputData
{
    public int playerId;
    public long timestamp;
    public float moveX;
    public float moveY;
    public bool fire;
    public bool ability;
}
