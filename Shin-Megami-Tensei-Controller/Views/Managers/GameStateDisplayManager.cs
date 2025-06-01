namespace Shin_Megami_Tensei;

public class GameStateDisplayManager
{
    private readonly GameUi _gameUi;
    private readonly GameUiUtilities _utilities;

    public GameStateDisplayManager(GameUi gameUi)
    {
        _gameUi = gameUi;
        _utilities = new GameUiUtilities();
    }

    public void DisplayGameState(GameStateDisplayInfo gameStateInfo)
    {
        PrintTeamsState(gameStateInfo.GetTeam1(), gameStateInfo.GetTeam2());
        _gameUi.PrintLine();
        PrintTurnInfo(gameStateInfo.GetCurrentTeam());
        _gameUi.PrintLine();
        ShowTeamOrder(gameStateInfo.GetCurrentTeam());
        _gameUi.PrintLine();
    }

    public void PrintPlayerRound(Team currentTeam)
    {
        string playerInfo = $"Ronda de {currentTeam.GetSamuraiName()} ({currentTeam.GetPlayer()})";
        _gameUi.WriteLine(playerInfo);
    }

    public void PrintTeamsState(Team team1, Team team2)
    {
        PrintTeamState(team1);
        PrintTeamState(team2);
    }

    public void PrintTeamState(Team team)
    {
        PrintTeamHeader(team);
        PrintSamuraiState(team);
        PrintTeamMonsters(team);
    }

    public void PrintTurnInfo(Team currentTeam)
    {
        _gameUi.WriteLine($"Full Turns: {currentTeam.GetMaxFullTurns() - currentTeam.GetFullTurns()}");
        _gameUi.WriteLine($"Blinking Turns: {currentTeam.GetBlinkingTurns()}");
    }

    public void ShowTeamOrder(Team currentTeam)
    {
        _gameUi.WriteLine("Orden:");
        DisplayOrderListUnits(currentTeam);
    }

    public void DisplayWinner(Team team1, Team team2, int winnerTeam)
    {
        var winningTeam = GetWinningTeam(team1, team2, winnerTeam);
        _gameUi.WriteLine($"Ganador: {winningTeam.GetSamuraiName()} ({winningTeam.GetPlayer()})");
    }

    private void PrintTeamHeader(Team team)
    {
        _gameUi.WriteLine($"Equipo de {team.GetSamuraiName()} ({team.GetPlayer()})");
    }

    private void PrintSamuraiState(Team team)
    {
        if (team.HasSamurai())
        {
            var samurai = team.GetSamurai();
            _gameUi.WriteLine($"A-{samurai.GetName()} HP:{samurai.GetCurrentHp()}/{samurai.GetMaxHp()} MP:{samurai.GetCurrentMp()}/{samurai.GetMaxMp()}");
        }
        else
        {
            _gameUi.WriteLine("A-");
        }
    }

    private void PrintTeamMonsters(Team team)
    {
        char monsterLabel = 'B';
        for (int monsterIndex = 0; monsterIndex < 3; monsterIndex++)
        {
            PrintMonsterState(team, monsterIndex, monsterLabel);
            monsterLabel++;
        }
    }

    private void PrintMonsterState(Team team, int monsterIndex, char monsterLabel)
    {
        if (team.IsPositionValid(monsterIndex))
        {
            var monster = team.GetMonsterAtPosition(monsterIndex);
            PrintMonsterInfo(monster, monsterLabel);
        }
        else
        {
            PrintEmptyMonsterSlot(monsterLabel);
        }
    }

    private void PrintMonsterInfo(Monster monster, char monsterLabel)
    {
        if (monster.IsDead())
            PrintDeadMonster(monsterLabel);
        else
            PrintAliveMonster(monster, monsterLabel);
    }

    private void PrintDeadMonster(char monsterLabel)
    {
        _gameUi.WriteLine($"{monsterLabel}-");
    }

    private void PrintAliveMonster(Monster monster, char monsterLabel)
    {
        _gameUi.WriteLine($"{monsterLabel}-{monster.GetName()} HP:{monster.GetCurrentHp()}/{monster.GetMaxHp()} MP:{monster.GetCurrentMp()}/{monster.GetMaxMp()}");
    }

    private void PrintEmptyMonsterSlot(char monsterLabel)
    {
        _gameUi.WriteLine($"{monsterLabel}-");
    }

    private void DisplayOrderListUnits(Team currentTeam)
    {
        var orderList = currentTeam.GetOrderList();
        for (int unitIndex = 0; unitIndex < orderList.Count; unitIndex++)
        {
            var unit = orderList[unitIndex];
            string unitName = _utilities.GetUnitName(unit);
            _gameUi.WriteLine($"{unitIndex + 1}-{unitName}");
        }
    }

    private Team GetWinningTeam(Team team1, Team team2, int winnerTeam)
    {
        return winnerTeam == 1 ? team1 : team2;
    }
}