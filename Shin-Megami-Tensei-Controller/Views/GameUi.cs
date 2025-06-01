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
        string playerInfo = $"Ronda de {currentTeam.GetSamurai().GetName()} ({currentTeam.GetPlayer()})";
        _view.WriteLine(playerInfo);
    }

    public void DisplayGameState(GameStateDisplayInfo gameStateInfo)
    {
        PrintTeamsState(gameStateInfo.GetTeam1(), gameStateInfo.GetTeam2());
        PrintLine();
        PrintTurnInfo(gameStateInfo.GetCurrentTeam());
        PrintLine();
        ShowTeamOrder(gameStateInfo.GetCurrentTeam());
        PrintLine();
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

    private void PrintTeamHeader(Team team)
    {
        _view.WriteLine($"Equipo de {team.GetSamurai()?.GetName()} ({team.GetPlayer()})");
    }

    private void PrintSamuraiState(Team team)
    {
        if (team.GetSamurai() != null)
        {
            var samurai = team.GetSamurai();
            _view.WriteLine($"A-{samurai.GetName()} HP:{samurai.GetCurrentHp()}/{samurai.GetMaxHp()} MP:{samurai.GetCurrentMp()}/{samurai.GetMaxMp()}");
        }
        else
        {
            _view.WriteLine("A-");
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
        var units = team.GetUnits();
        if (IsMonsterIndexValid(units, monsterIndex))
        {
            var monster = units[monsterIndex];
            PrintMonsterInfo(monster, monsterLabel);
        }
        else
        {
            PrintEmptyMonsterSlot(monsterLabel);
        }
    }

    private bool IsMonsterIndexValid(List<Monster> units, int monsterIndex)
    {
        return monsterIndex < units.Count;
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
        _view.WriteLine($"{monsterLabel}-");
    }

    private void PrintAliveMonster(Monster monster, char monsterLabel)
    {
        _view.WriteLine($"{monsterLabel}-{monster.GetName()} HP:{monster.GetCurrentHp()}/{monster.GetMaxHp()} MP:{monster.GetCurrentMp()}/{monster.GetMaxMp()}");
    }

    private void PrintEmptyMonsterSlot(char monsterLabel)
    {
        _view.WriteLine($"{monsterLabel}-");
    }

    public void PrintTurnInfo(Team currentTeam)
    {
        _view.WriteLine($"Full Turns: {currentTeam.GetMaxFullTurns() - currentTeam.GetFullTurns()}");
        _view.WriteLine($"Blinking Turns: {currentTeam.GetBlinkingTurns()}");
    }

    public void ShowTeamOrder(Team currentTeam)
    {
        _view.WriteLine("Orden:");
        DisplayOrderListUnits(currentTeam);
    }

    private void DisplayOrderListUnits(Team currentTeam)
    {
        var orderList = currentTeam.GetOrderList();
        for (int unitIndex = 0; unitIndex < orderList.Count; unitIndex++)
        {
            var unit = orderList[unitIndex];
            string unitName = GetUnitName(unit);
            _view.WriteLine($"{unitIndex + 1}-{unitName}");
        }
    }

    public void ShowFiles(string[] files)
    {
        for (int fileIndex = 0; fileIndex < files.Length; fileIndex++)
        {
            string fileName = ExtractFileName(files[fileIndex]);
            _view.WriteLine($"{fileIndex}: {fileName}");
        }
    }

    private string ExtractFileName(string filePath)
    {
        return Path.GetFileName(filePath);
    }

    public void PrintTurnsUsed(TurnUsageInfo turnUsage)
    {
        PrintLine();
        _view.WriteLine($"Se han consumido {turnUsage.FullTurnsUsed} Full Turn(s) y {turnUsage.BlinkingTurnsUsed} Blinking Turn(s)");
        _view.WriteLine($"Se han obtenido {turnUsage.BlinkingTurnsGained} Blinking Turn(s)");
    }

    public void DisplayWinner(Team team1, Team team2, int winnerTeam)
    {
        var winningTeam = GetWinningTeam(team1, team2, winnerTeam);
        _view.WriteLine($"Ganador: {winningTeam.GetSamurai().GetName()} ({winningTeam.GetPlayer()})");
    }

    private Team GetWinningTeam(Team team1, Team team2, int winnerTeam)
    {
        return winnerTeam == 1 ? team1 : team2;
    }

    public string GetUnitName(IUnit unit)
    {
        return unit.GetName();
    }

    public void ShowHpResult(string targetName, int remainingHp, int originalHp)
    {
        _view.WriteLine($"{targetName} termina con HP:{remainingHp}/{originalHp}");
    }
    
    public void ShowAttack(string attackerName, string actionType, string targetName)
    {
        _view.WriteLine($"{attackerName} {actionType} a {targetName}");
    }
    
    public void ShowAffinityResponse(AffinityResponseInfo affinityInfo)
    {
        switch (affinityInfo.Affinity)
        {
            case "Rs":
                ShowResistResponse(affinityInfo.AttackerName, affinityInfo.TargetName, affinityInfo.Damage);
                break;
            case "Wk":
                ShowWeakResponse(affinityInfo.AttackerName, affinityInfo.TargetName, affinityInfo.Damage);
                break;
            case "Nu":
                ShowNullResponse(affinityInfo.AttackerName, affinityInfo.TargetName);
                break;
            case "Dr":
                ShowDrainResponse(affinityInfo.TargetName, affinityInfo.Damage);
                break;
            case "Rp":
                ShowRepelResponse(affinityInfo.AttackerName, affinityInfo.TargetName, affinityInfo.Damage);
                break;
            default:
                ShowNormalDamageResponse(affinityInfo.TargetName, affinityInfo.Damage);
                break;
        }
    }

    private void ShowResistResponse(string attackerName, string targetName, int damage)
    {
        _view.WriteLine($"{targetName} es resistente el ataque de {attackerName}");
        ShowDamageReceived(targetName, damage);
    }

    private void ShowWeakResponse(string attackerName, string targetName, int damage)
    {
        _view.WriteLine($"{targetName} es débil contra el ataque de {attackerName}");
        ShowDamageReceived(targetName, damage);
    }

    private void ShowNullResponse(string attackerName, string targetName)
    {
        _view.WriteLine($"{targetName} bloquea el ataque de {attackerName}");
    }

    private void ShowDrainResponse(string targetName, int damage)
    {
        _view.WriteLine($"{targetName} absorbe {damage} daño");
    }

    private void ShowRepelResponse(string attackerName, string targetName, int damage)
    {
        _view.WriteLine($"{targetName} devuelve {damage} daño a {attackerName}");
    }

    private void ShowNormalDamageResponse(string targetName, int damage)
    {
        ShowDamageReceived(targetName, damage);
    }

    private void ShowDamageReceived(string targetName, int damage)
    {
        _view.WriteLine($"{targetName} recibe {damage} de daño");
    }
    
    public int GetSamuraiActionOptions(Samurai samurai)
    {
        ShowSamuraiActionPrompt(samurai);
        DisplaySamuraiActionOptions();
        return int.Parse(_view.ReadLine());
    }

    private void ShowSamuraiActionPrompt(Samurai samurai)
    {
        _view.WriteLine($"Seleccione una acción para {samurai.GetName()}");
    }

    private void DisplaySamuraiActionOptions()
    {
        _view.WriteLine("1: Atacar");
        _view.WriteLine("2: Disparar");
        _view.WriteLine("3: Usar Habilidad");
        _view.WriteLine("4: Invocar");
        _view.WriteLine("5: Pasar Turno");
        _view.WriteLine("6: Rendirse");
    }

    public int GetMonsterActionOptions(Monster monster)
    {
        ShowMonsterActionPrompt(monster);
        DisplayMonsterActionOptions();
        return int.Parse(_view.ReadLine());
    }

    private void ShowMonsterActionPrompt(Monster monster)
    {
        _view.WriteLine($"Seleccione una acción para {monster.GetName()}");
    }

    private void DisplayMonsterActionOptions()
    {
        _view.WriteLine("1: Atacar");
        _view.WriteLine("2: Usar Habilidad");
        _view.WriteLine("3: Invocar");
        _view.WriteLine("4: Pasar Turno");
    }

    public int DisplaySummonMenu(List<Monster> availableMonsters)
    {
        WriteLine("Seleccione un monstruo para invocar");
        DisplayAvailableMonsters(availableMonsters);
        DisplaySummonCancelOption(availableMonsters.Count);
        return int.Parse(ReadLine());
    }

    private void DisplayAvailableMonsters(List<Monster> availableMonsters)
    {
        for (int i = 0; i < availableMonsters.Count; i++)
        {
            Monster monster = availableMonsters[i];
            WriteLine($"{i + 1}-{monster.GetName()} HP:{monster.GetCurrentHp()}/{monster.GetMaxHp()} MP:{monster.GetCurrentMp()}/{monster.GetMaxMp()}");
        }
    }

    private void DisplaySummonCancelOption(int monstersCount)
    {
        WriteLine($"{monstersCount + 1}-Cancelar");
    }

    public int DisplayPositionMenu(Team currentTeam)
    {
        WriteLine("Seleccione una posición para invocar");
        DisplayPositionOptions(currentTeam);
        WriteLine("4-Cancelar");
        return int.Parse(ReadLine());
    }

    private void DisplayPositionOptions(Team currentTeam)
    {
        for (int i = 0; i < 3; i++)
        {
            DisplayPositionSlot(currentTeam, i);
        }
    }

    private void DisplayPositionSlot(Team currentTeam, int position)
    {
        if (IsPositionOccupiedByAliveMonster(currentTeam, position))
        {
            DisplayOccupiedPosition(currentTeam, position);
        }
        else
        {
            DisplayEmptyPosition(position);
        }
    }

    private bool IsPositionOccupiedByAliveMonster(Team currentTeam, int position)
    {
        var units = currentTeam.GetUnits();
        return position < units.Count && !units[position].IsDead();
    }

    private void DisplayOccupiedPosition(Team currentTeam, int position)
    {
        var units = currentTeam.GetUnits();
        Monster monster = units[position];
        WriteLine($"{position+1}-{monster.GetName()} HP:{monster.GetCurrentHp()}/{monster.GetMaxHp()} MP:{monster.GetCurrentMp()}/{monster.GetMaxMp()} (Puesto {position+2})");
    }

    private void DisplayEmptyPosition(int position)
    {
        WriteLine($"{position+1}-Vacío (Puesto {position+2})");
    }

    public void DisplaySummonSuccess(string monsterName)
    {
        WriteLine($"{monsterName} ha sido invocado");
    }
    
    public void ShowHealMessage(IUnit target, int healAmount)
    {
        string targetName = GetUnitName(target);
        WriteLine($"{targetName} recibe {healAmount} de HP");
    }
    
    public void ShowHealingAction(string healerName, string targetName)
    {
        _view.WriteLine($"{healerName} cura a {targetName}");
    }

    public void ShowHealAmountReceived(string targetName, int healAmount)
    {
        _view.WriteLine($"{targetName} recibe {healAmount} de HP");
    }

    public void ShowReviveAction(string reviverName, string targetName)
    {
        _view.WriteLine($"{reviverName} revive a {targetName}");
    }

    public void ShowSurrenderMessage(string samuraiName, string playerName)
    {
        _view.WriteLine($"{samuraiName} ({playerName}) se rinde");
    }

    public void ShowSkillSelectionPrompt(string unitName)
    {
        _view.WriteLine($"Seleccione una habilidad para que {unitName} use");
    }

    public void ShowAffordableSkill(int optionNumber, string skillName, int skillCost)
    {
        _view.WriteLine($"{optionNumber}-{skillName} MP:{skillCost}");
    }

    public void ShowSkillCancelOption(int optionNumber)
    {
        _view.WriteLine($"{optionNumber}-Cancelar");
    }
}