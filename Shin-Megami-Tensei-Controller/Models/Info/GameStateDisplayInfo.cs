namespace Shin_Megami_Tensei;

public class GameStateDisplayInfo
{
    public Team CurrentTeam { get; }
    public GameTeamsInfo Teams { get; }
    public int CurrentTurn { get; }
    
    public Team GetCurrentTeam() => CurrentTeam;
    public Team GetTeam1() => Teams.Team1;
    public Team GetTeam2() => Teams.Team2;
    public int GetCurrentTurn() => CurrentTurn;
    
    public GameStateDisplayInfo(Team currentTeam, GameTeamsInfo teams, int currentTurn)
    {
        CurrentTeam = currentTeam;
        Teams = teams;
        CurrentTurn = currentTurn;
    }
}