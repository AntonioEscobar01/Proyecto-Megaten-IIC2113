// ...existing code...

public bool CanInitializeTeamsFromFiles()
{
    Team playerTeam = LoadPlayerTeam();
    if (playerTeam == null)
    {
        return false;
    }

    Team enemyTeam = LoadEnemyTeam();
    if (enemyTeam == null)
    {
        return false;
    }

    _playerTeam = playerTeam;
    _enemyTeam = enemyTeam;
    return true;
}

private Team LoadPlayerTeam()
{
    return _teamLoadManager.LoadTeamFromFile("player_team.txt");
}

private Team LoadEnemyTeam()
{
    return _teamLoadManager.LoadTeamFromFile("enemy_team.txt");
}

// ...existing code...
