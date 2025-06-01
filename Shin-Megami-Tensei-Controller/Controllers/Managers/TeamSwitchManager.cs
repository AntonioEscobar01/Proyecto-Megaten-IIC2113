namespace Shin_Megami_Tensei;

public class TeamSwitchManager
{
    private readonly Team _teamPlayer1;
    private readonly Team _teamPlayer2;
    private readonly TurnManager _turnManager;

    public TeamSwitchManager(Team teamPlayer1, Team teamPlayer2, TurnManager turnManager)
    {
        _teamPlayer1 = teamPlayer1;
        _teamPlayer2 = teamPlayer2;
        _turnManager = turnManager;
    }

    public Team GetCurrentTeam()
    {
        return (_turnManager.GetCurrentTurn() % 2 != 0) ? _teamPlayer1 : _teamPlayer2;
    }

    public Team GetEnemyTeam()
    {
        return (_turnManager.GetCurrentTurn() % 2 != 0) ? _teamPlayer2 : _teamPlayer1;
    }
}