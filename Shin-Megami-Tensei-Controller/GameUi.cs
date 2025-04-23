using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class GameUi
{
    private readonly View _view;
    private const string SEPARATOR = "----------------------------------------";

    public GameUi(View view)
    {
        _view = view;
    }

    public void PrintLine()
    {
        _view.WriteLine(SEPARATOR);
    }

    public string ReadLine()
    {
        return _view.ReadLine();
    }

    public void WriteLine(string text)
    {
        _view.WriteLine(text);
    }

    public void PrintPlayerRound(Team currentTeam)
    {
        string playerInfo = $"Ronda de {currentTeam.Samurai.Name} ({currentTeam.Player})";
        _view.WriteLine(playerInfo);
    }

    public void DisplayGameState(Team currentTeam, Team team1, Team team2, int currentTurn)
    {
        PrintTeamsState(team1, team2);
        PrintLine();
        PrintTurnInfo(currentTeam);
        PrintLine();
        ShowTeamOrder(currentTeam);
        PrintLine();
    }

    public void PrintTeamsState(Team team1, Team team2)
    {
        PrintTeamState(team1);
        PrintTeamState(team2);
    }

    public void PrintTeamState(Team team)
    {
        _view.WriteLine($"Equipo de {team.Samurai?.Name} ({team.Player})");

        if (team.Samurai != null)
        {
            _view.WriteLine($"A-{team.Samurai.Name} HP:{team.Samurai.Hp}/{team.Samurai.OriginalHp} MP:{team.Samurai.Mp}/{team.Samurai.OriginalMp}");
        }
        else
        {
            _view.WriteLine("A-");
        }

        PrintTeamMonsters(team);
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
        if (monsterIndex < team.Units.Count)
        {
            var monster = team.Units[monsterIndex];
            if (monster.IsDead())
                _view.WriteLine($"{monsterLabel}-");
            else
                _view.WriteLine($"{monsterLabel}-{monster.Name} HP:{monster.Hp}/{monster.OriginalHp} MP:{monster.Mp}/{monster.OriginalMp}");
        }
        else
        {
            _view.WriteLine($"{monsterLabel}-");
        }
    }

    public void PrintTurnInfo(Team currentTeam)
    {
        _view.WriteLine($"Full Turns: {currentTeam.MaxFullTurns - currentTeam.FullTurns}");
        _view.WriteLine($"Blinking Turns: {currentTeam.BlinkingTurns}");
    }

    public void ShowTeamOrder(Team currentTeam)
    {
        _view.WriteLine("Orden:");

        for (int unitIndex = 0; unitIndex < currentTeam.OrderList.Count; unitIndex++)
        {
            var unit = currentTeam.OrderList[unitIndex];
            string unitName = GetUnitName(unit);
            _view.WriteLine($"{unitIndex + 1}-{unitName}");
        }
    }

    public void ShowFiles(string[] files)
    {
        for (int fileIndex = 0; fileIndex < files.Length; fileIndex++)
        {
            string fileName = Path.GetFileName(files[fileIndex]);
            _view.WriteLine($"{fileIndex}: {fileName}");
        }
    }

    public void PrintTurnsUsed(Team currentTeam, int fullTurnsUsed, int blinkingTurnsUsed, int blinkingTurnsGained)
    {
        PrintLine();
        _view.WriteLine($"Se han consumido {fullTurnsUsed} Full Turn(s) y {blinkingTurnsUsed} Blinking Turn(s)");
        _view.WriteLine($"Se han obtenido {blinkingTurnsGained} Blinking Turn(s)");
    }

    public void DisplayWinner(Team team1, Team team2, int winnerTeam)
    {
        if (winnerTeam == 1)
            _view.WriteLine($"Ganador: {team1.Samurai.Name} ({team1.Player})");
        else
            _view.WriteLine($"Ganador: {team2.Samurai.Name} ({team2.Player})");
    }

    public string GetUnitName(object unit)
    {
        return unit is Samurai samurai ? samurai.Name : ((Monster)unit).Name;
    }

    public void ShowDamageResult(string targetName, int remainingHp, int originalHp)
    {
        _view.WriteLine($"{targetName} termina con HP:{remainingHp}/{originalHp}");
    }
    
    public void ShowAttack(string attackerName, string actionType, string targetName)
    {
        _view.WriteLine($"{attackerName} {actionType} a {targetName}");
    }
    
    public void ShowAffinityResponse(string attackerName, string affinity, string targetName, int damage)
    {
        if (affinity == "Rs")
        {
            _view.WriteLine($"{targetName} es resistente el ataque de {attackerName}");
            _view.WriteLine($"{targetName} recibe {damage} de daño");
        }
        else if (affinity == "Wk")
        {
            _view.WriteLine($"{targetName} es débil contra el ataque de {attackerName}");
            _view.WriteLine($"{targetName} recibe {damage} de daño");
        }
        else if (affinity == "Nu")
        {
            _view.WriteLine($"{targetName} bloquea el ataque de {attackerName}");
        }
        else if (affinity == "Dr")
        {
            _view.WriteLine($"{targetName} absorbe {damage} daño");
        }
        else if (affinity == "Rp")
        {
            _view.WriteLine($"{targetName} devuelve {damage} daño a {attackerName}");
        }
        else
        {
            _view.WriteLine($"{targetName} recibe {damage} de daño");
        }
    }
    
    public int GetSamuraiActionOptions(Samurai samurai)
    {
        _view.WriteLine($"Seleccione una acción para {samurai.Name}");
        _view.WriteLine("1: Atacar");
        _view.WriteLine("2: Disparar");
        _view.WriteLine("3: Usar Habilidad");
        _view.WriteLine("4: Invocar");
        _view.WriteLine("5: Pasar Turno");
        _view.WriteLine("6: Rendirse");
        return int.Parse(_view.ReadLine());
    }

    public int GetMonsterActionOptions(Monster monster)
    {
        _view.WriteLine($"Seleccione una acción para {monster.Name}");
        _view.WriteLine("1: Atacar");
        _view.WriteLine("2: Usar Habilidad");
        _view.WriteLine("3: Invocar");
        _view.WriteLine("4: Pasar Turno");
        return int.Parse(_view.ReadLine());
    }
    
    public void PrintTurnsUsed(Team currentTeam)
    {
        // Llamada al método con valores predeterminados
        PrintTurnsUsed(currentTeam, 0, 0, 0);
    }
}