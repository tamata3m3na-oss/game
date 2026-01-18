/// <summary>
/// Application game state (Boot, Lobby, Match, Result)
/// 
/// NOTE: This is different from NetworkManager.GameState which represents
/// server-sent game snapshot data containing player positions, health, etc.
/// </summary>
public enum AppGameState
{
    Boot = 0,
    Lobby = 1,
    Match = 2,
    Result = 3
}