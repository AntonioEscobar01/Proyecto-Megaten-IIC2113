namespace Shin_Megami_Tensei;

public class GameStateDisplayInfo
{
    private readonly Team _currentTeam;
    private readonly Team _team1;
    private readonly Team _team2;
    private readonly int _currentTurn;
    
    public GameStateDisplayInfo(Team currentTeam, Team team1, Team team2, int currentTurn)
    {
        _currentTeam = currentTeam;
        _team1 = team1;
        _team2 = team2;
        _currentTurn = currentTurn;
    }

    public Team GetCurrentTeam() => _currentTeam;
    public Team GetTeam1() => _team1;
    public Team GetTeam2() => _team2;
    public int GetCurrentTurn() => _currentTurn;
}