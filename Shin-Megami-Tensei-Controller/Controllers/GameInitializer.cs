namespace Shin_Megami_Tensei;

public class GameInitializer
{
    private readonly TeamLoadManager _teamLoadManager;

    public GameInitializer(TeamLoadManager teamLoadManager)
    {
        _teamLoadManager = teamLoadManager;
    }

    public GameInitializationResult InitializeTeams()
    {
        var loadedTeams = _teamLoadManager.LoadTeamsFromFile();
        if (!loadedTeams.AreTeamsValid)
            return new GameInitializationResult(null, null, false);

        var teamPlayer1 = loadedTeams.TeamPlayer1;
        var teamPlayer2 = loadedTeams.TeamPlayer2;
        
        ConfigureTeamOrders(teamPlayer1, teamPlayer2);
        return new GameInitializationResult(teamPlayer1, teamPlayer2, true);
    }

    private void ConfigureTeamOrders(Team teamPlayer1, Team teamPlayer2)
    {
        teamPlayer1.InitializeOrderList();
        teamPlayer2.InitializeOrderList();
        teamPlayer1.SetMaxFullTurns();
        teamPlayer2.SetMaxFullTurns();
    }
}