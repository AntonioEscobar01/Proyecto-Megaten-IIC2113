namespace Shin_Megami_Tensei;

public class GameInitializerController
{
    private readonly TeamLoadManager _teamLoadManager;

    public GameInitializerController(TeamLoadManager teamLoadManager)
    {
        _teamLoadManager = teamLoadManager;
    }

    public GameInitializationResultData InitializeTeams()
    {
        var loadedTeams = _teamLoadManager.LoadTeamsFromFile();
        if (!loadedTeams.AreTeamsValid)
            return new GameInitializationResultData(null, null, false);

        var teamPlayer1 = loadedTeams.TeamPlayer1;
        var teamPlayer2 = loadedTeams.TeamPlayer2;
        
        ConfigureTeamOrders(teamPlayer1, teamPlayer2);
        return new GameInitializationResultData(teamPlayer1, teamPlayer2, true);
    }

    private void ConfigureTeamOrders(Team teamPlayer1, Team teamPlayer2)
    {
        teamPlayer1.InitializeOrderList();
        teamPlayer2.InitializeOrderList();
        teamPlayer1.SetMaxFullTurns();
        teamPlayer2.SetMaxFullTurns();
    }
}