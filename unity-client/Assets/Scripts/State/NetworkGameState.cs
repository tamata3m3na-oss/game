using System;

/// <summary>
حالة اللعبة الكاملة (قابلة للتعديل)
تستخدم داخلياً في الـ Repository
/// </summary>
[Serializable]
public class NetworkGameState
{
    public int matchId;
    public PlayerStateData player1;
    public PlayerStateData player2;
    public int tick;
    public long timestamp;
    public int winner;
    public string status;

    public NetworkGameState() { }

    public NetworkGameState(NetworkGameState other)
    {
        if (other == null) return;

        this.matchId = other.matchId;
        this.tick = other.tick;
        this.timestamp = other.timestamp;
        this.winner = other.winner;
        this.status = other.status;

        if (other.player1 != null)
        {
            this.player1 = new PlayerStateData(other.player1);
        }
        if (other.player2 != null)
        {
            this.player2 = new PlayerStateData(other.player2);
        }
    }

    public override string ToString()
    {
        return $"Match {matchId}, Tick {tick}, Winner={winner}, Status={status}";
    }
}