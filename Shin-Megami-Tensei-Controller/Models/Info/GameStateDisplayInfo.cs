namespace Shin_Megami_Tensei;

public class GameStateDisplayInfo
{
    public Team CurrentTeam { get; }
    public Team Team1 { get; }
    public Team Team2 { get; }
    public int CurrentTurn { get; }
    
    public GameStateDisplayInfo(Team currentTeam, Team team1, Team team2, int currentTurn)
    {
        CurrentTeam = currentTeam;
        Team1 = team1;
        Team2 = team2;
        CurrentTurn = currentTurn;
    }
}