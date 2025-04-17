using Shin_Megami_Tensei_View;
namespace Shin_Megami_Tensei;

public class Game
{
    private readonly View _view;
    private readonly string _teamsFolder;
    private int _currentTurn;
    private SkillsManager _skillsManager;
    private Boolean _gameOver;
    private int _winnerTeam;

    public Game(View view, string teamsFolderInitializer)
    {
        _view = view;
        _teamsFolder = teamsFolderInitializer;
        _currentTurn = 1;
        _gameOver = false;
        _winnerTeam = 0;
    }

    public void Play()
    {
        _view.WriteLine("Elige un archivo para cargar los equipos");
        var teamFiles = Directory.GetFiles(_teamsFolder);
        ShowFiles(teamFiles);
        _skillsManager = new SkillsManager();
        var (teamPlayer1, teamPlayer2) = InitializeTeams(teamFiles);
        if (teamPlayer1 == null || teamPlayer2 == null)
            return;
        PrintLine();
        PrintPlayerRound(teamPlayer1, teamPlayer2);
        PrintLine();
        RunGameLoop(teamPlayer1, teamPlayer2);
        DisplayWinner(teamPlayer1, teamPlayer2);
    }

    private (Team, Team) InitializeTeams(string[] teamFiles)
    {
        int chosenFileIndex = int.Parse(_view.ReadLine());
        string chosenFile = teamFiles[chosenFileIndex];
        var teamsInfo = File.ReadAllLines(chosenFile);
        var (unitsPlayer1, unitsPlayer2) = SplitTeams(teamsInfo);
        var teamPlayer1 = new Team("J1");
        var teamPlayer2 = new Team("J2");
        bool teamOneValid = teamPlayer1.IsValidTeam(unitsPlayer1);
        bool teamTwoValid = teamPlayer2.IsValidTeam(unitsPlayer2);
        teamPlayer1.InitializeOrderList();
        teamPlayer2.InitializeOrderList();
        teamPlayer1.SetMaxFullTurns();
        teamPlayer2.SetMaxFullTurns();
        if (!teamOneValid || !teamTwoValid)
        {
            _view.WriteLine("Archivo de equipos inválido");
            return (null, null);
        }
        return (teamPlayer1, teamPlayer2);
    }

    private void RunGameLoop(Team teamPlayer1, Team teamPlayer2)
    {
        while (_gameOver == false)
        {
            var currentTeam = (_currentTurn % 2 != 0) ? teamPlayer1 : teamPlayer2;
            currentTeam.SetMaxFullTurns();

            DisplayGameState(teamPlayer1, teamPlayer2);
            ProcessTurn(teamPlayer1, teamPlayer2);

            CheckGameOver(teamPlayer1, teamPlayer2);
            HandleTurnTransition(teamPlayer1, teamPlayer2, currentTeam);

            teamPlayer1.RemoveDeadUnits();
            teamPlayer2.RemoveDeadUnits();
        }
    }

    private void DisplayGameState(Team teamPlayer1, Team teamPlayer2)
    {
        PrintTeamsState(teamPlayer1, teamPlayer2);
        PrintLine();
        PrintTurnInfo(teamPlayer1, teamPlayer2);
        PrintLine();
        ShowTeamOrder(teamPlayer1, teamPlayer2);
        PrintLine();
    }

    private void ProcessTurn(Team teamPlayer1, Team teamPlayer2)
    {
        GameFlow(teamPlayer1, teamPlayer2);
        PrintLine();
        if (_gameOver != true)
        {
            PrintTurnsUsed(GetCurrentTeam(teamPlayer1, teamPlayer2));
            PrintLine();
        }
    }

    private void CheckGameOver(Team teamPlayer1, Team teamPlayer2)
    {
        if (teamPlayer1.AllUnitsDead())
        {
            _gameOver = true;
            _winnerTeam = 2;
        }
        if (teamPlayer2.AllUnitsDead())
        {
            _gameOver = true;
            _winnerTeam = 1;
        }
    }

    private void HandleTurnTransition(Team teamPlayer1, Team teamPlayer2, Team currentTeam)
    {
        if (currentTeam.HasCompletedAllTurns() && _gameOver == false)
        {
            _currentTurn++;
            PrintPlayerRound(teamPlayer1, teamPlayer2);
            PrintLine();
            currentTeam.ResetTurns();
        }
    }

    private void GameFlow(Team teamPlayer1, Team teamPlayer2)
    {
        var currentTeam = GetCurrentTeam(teamPlayer1, teamPlayer2);
        var currentUnit = currentTeam.OrderList[0];
        int selectedAction = GetUnitAction(currentUnit);
        PrintLine();
        ProcessAction(teamPlayer1, teamPlayer2, currentTeam, currentUnit, selectedAction);
    }

    private int GetUnitAction(object currentUnit)
    {
        if (currentUnit is Samurai samurai)
        {
            return SamuraiActionOptions(samurai);
        }
        else
        {
            return MonsterActionOptions((Monster)currentUnit);
        }
    }

    private void ProcessAction(Team teamPlayer1, Team teamPlayer2, Team currentTeam, object currentUnit, int action)
    {
        int actionCode = currentUnit is Monster ? action + 10 : action;
        switch (actionCode)
        {
            case 1: case 2: case 11: 
                ProcessAttackAction(teamPlayer1, teamPlayer2, currentTeam, currentUnit, action); break;
            case 3: case 12:
                ProcessSkillAction(teamPlayer1, teamPlayer2, currentUnit); break;
            case 4: case 13:
                ProcessInvokeAction(); break;
            case 5: case 14:
                ProcessPassTurnAction(currentTeam); break;
            case 6:
                ProcessSurrenderAction(currentTeam); break;
        }
    }

    private void ProcessAttackAction(Team teamPlayer1, Team teamPlayer2, Team currentTeam, object currentUnit, int action)
    {
        var enemyTeam = GetEnemyTeam(teamPlayer1, teamPlayer2);
        int targetIndex = ChooseUnitToAttack(enemyTeam, currentUnit);
        if (targetIndex == 5)
        {
            PrintLine();
            GameFlow(teamPlayer1, teamPlayer2);
            return;
        }
        PrintLine();
        object targetUnit = GetTarget(enemyTeam, targetIndex);

        if (action == 1)
            Attack(currentUnit, targetUnit);
        else
            Shoot(currentUnit, targetUnit);
        currentTeam.RotateOrderList();
        currentTeam.TurnComplete();
    }

    private void ProcessSkillAction(Team teamPlayer1, Team teamPlayer2, object currentUnit)
    {
        var (selectedIndex, abilities) = UseSkill(currentUnit);
        PrintLine();
        GameFlow(teamPlayer1, teamPlayer2);
    }

    private void ProcessInvokeAction()
    {
    }

    private void ProcessPassTurnAction(Team currentTeam)
    {
        currentTeam.RotateOrderList();
        currentTeam.TurnComplete();
    }

    private void ProcessSurrenderAction(Team currentTeam)
    {
        _view.WriteLine($"{currentTeam.Samurai.Name} ({currentTeam.Player}) se rinde");
        _gameOver = true;
        _winnerTeam = (_currentTurn % 2 != 0) ? 2 : 1;
    }

    private Team GetCurrentTeam(Team teamPlayer1, Team teamPlayer2)
    {
        return (_currentTurn % 2 != 0) ? teamPlayer1 : teamPlayer2;
    }

    private Team GetEnemyTeam(Team teamPlayer1, Team teamPlayer2)
    {
        return (_currentTurn % 2 != 0) ? teamPlayer2 : teamPlayer1;
    }

    private void PrintLine()
    {
        _view.WriteLine("----------------------------------------");
    }

    private void PrintPlayerRound(Team teamPlayer1, Team teamPlayer2)
    {
        string playerInfo = (_currentTurn % 2 != 0)
            ? $"Ronda de {teamPlayer1.Samurai.Name} ({teamPlayer1.Player})"
            : $"Ronda de {teamPlayer2.Samurai.Name} ({teamPlayer2.Player})";
        _view.WriteLine(playerInfo);
    }

    private void PrintTurnInfo(Team teamPlayer1, Team teamPlayer2)
    {
        if (_currentTurn % 2 != 0)
        {
            _view.WriteLine($"Full Turns: {teamPlayer1.MaxFullTurns - teamPlayer1.FullTurns}");
            _view.WriteLine($"Blinking Turns: {teamPlayer1.BlinkingTurns}");
        }
        else
        {
            _view.WriteLine($"Full Turns: {teamPlayer2.MaxFullTurns - teamPlayer2.FullTurns}");
            _view.WriteLine($"Blinking Turns: {teamPlayer2.BlinkingTurns}");
        }
    }

    private void ShowTeamOrder(Team teamPlayer1, Team teamPlayer2)
    {
        Team currentTeam = (_currentTurn % 2 != 0) ? teamPlayer1 : teamPlayer2;

        _view.WriteLine("Orden:");

        for (int unitIndex = 0; unitIndex < currentTeam.OrderList.Count; unitIndex++)
        {
            var unit = currentTeam.OrderList[unitIndex];
            string unitName = unit is Samurai ? ((Samurai)unit).Name : ((Monster)unit).Name;
            _view.WriteLine($"{unitIndex + 1}-{unitName}");
        }
    }

    private void ShowFiles(string[] files)
    {
        int fileOptionNumber = 0;
        foreach (var file in files)
        {
            string fileName = Path.GetFileName(file);
            _view.WriteLine($"{fileOptionNumber}: {fileName}");
            fileOptionNumber++;
        }
    }

    private (List<string> unitsPlayer1, List<string> unitsPlayer2) SplitTeams(string[] teamsInfo)
    {
        var unitsPlayer1 = new List<string>();
        var unitsPlayer2 = new List<string>();
        bool isProcessingTeam1 = true;
        foreach (var line in teamsInfo[1..])
        {
            if (line == "Player 2 Team")
            {
                isProcessingTeam1 = false;
                continue;
            }
            if (isProcessingTeam1)
                unitsPlayer1.Add(line);
            else
                unitsPlayer2.Add(line);
        }
        return (unitsPlayer1, unitsPlayer2);
    }

    private void PrintTeamsState(Team teamPlayer1, Team teamPlayer2)
    {
        PrintTeamState(teamPlayer1);
        PrintTeamState(teamPlayer2);
    }

    private void PrintTeamState(Team team)
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
            monsterLabel++;
        }
    }

    private int SamuraiActionOptions(Samurai samurai)
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

    private int MonsterActionOptions(Monster monster)
    {
        _view.WriteLine($"Seleccione una acción para {monster.Name}");
        _view.WriteLine("1: Atacar");
        _view.WriteLine("2: Usar Habilidad");
        _view.WriteLine("3: Invocar");
        _view.WriteLine("4: Pasar Turno");
        return int.Parse(_view.ReadLine());
    }

    private int ChooseUnitToAttack(Team enemyTeam, object attacker)
    {
        string attackerName = attacker is Samurai ? ((Samurai)attacker).Name : ((Monster)attacker).Name;
        _view.WriteLine($"Seleccione un objetivo para {attackerName}");

        List<object> availableTargets = GetAvailableTargets(enemyTeam);
        DisplayTargets(availableTargets);

        return ProcessTargetSelection(availableTargets, enemyTeam);
    }

    private List<object> GetAvailableTargets(Team enemyTeam)
    {
        List<object> availableTargets = new List<object>();
        if (enemyTeam.Samurai != null && !enemyTeam.Samurai.IsDead())
        {
            availableTargets.Add(enemyTeam.Samurai);
        }
        int maxVisibleMonsters = Math.Min(enemyTeam.Units.Count, 3);
        for (int monsterIndex = 0; monsterIndex < maxVisibleMonsters; monsterIndex++)
        {
            var monster = enemyTeam.Units[monsterIndex];
            if (!monster.IsDead())
            {
                availableTargets.Add(monster);
            }
        }
        return availableTargets;
    }

    private void DisplayTargets(List<object> availableTargets)
    {
        for (int targetIndex = 0; targetIndex < availableTargets.Count; targetIndex++)
        {
            var target = availableTargets[targetIndex];
            if (target is Samurai samurai)
            {
                _view.WriteLine($"{targetIndex+1}-{samurai.Name} HP:{samurai.Hp}/{samurai.OriginalHp} MP:{samurai.Mp}/{samurai.OriginalMp}");
            }
            else if (target is Monster monster)
            {
                _view.WriteLine($"{targetIndex+1}-{monster.Name} HP:{monster.Hp}/{monster.OriginalHp} MP:{monster.Mp}/{monster.OriginalMp}");
            }
        }
        _view.WriteLine($"{availableTargets.Count + 1}-Cancelar");
    }

    private int ProcessTargetSelection(List<object> availableTargets, Team enemyTeam)
    {
        int selection = int.Parse(_view.ReadLine());

        if (selection == availableTargets.Count + 1)
            return 5;
        if (selection > 0 && selection <= availableTargets.Count)
        {
            var selectedTarget = availableTargets[selection - 1];
            if (selectedTarget is Samurai)
                return 1;
            else
                return enemyTeam.Units.IndexOf((Monster)selectedTarget) + 2;
        }
        return 5;
    }

    private void Attack(object attacker, object target)
    {
        int damage = CalculateDamage(attacker, 54);
        ApplyDamage(attacker, target, damage, "ataca");
    }

    private void Shoot(object attacker, object target)
    {
        int damage = CalculateDamage(attacker, 80);
        ApplyDamage(attacker, target, damage, "dispara");
    }

    private void ApplyDamage(object attacker, object target, int damage, string actionType)
    {
        string attackerName = GetUnitName(attacker);
        string targetName = GetUnitName(target);
        _view.WriteLine($"{attackerName} {actionType} a {targetName}");
        _view.WriteLine($"{targetName} recibe {damage} de daño");
        UpdateTargetHealth(target, damage);
    }

    private string GetUnitName(object unit)
    {
        return unit is Samurai samurai ? samurai.Name : ((Monster)unit).Name;
    }

    private void UpdateTargetHealth(object target, int damage)
    {
        if (target is Samurai samurai)
        {
            samurai.Hp -= damage;
            if (samurai.Hp < 0) samurai.Hp = 0;
            _view.WriteLine($"{samurai.Name} termina con HP:{samurai.Hp}/{samurai.OriginalHp}");
        }
        else if (target is Monster monster)
        {
            monster.Hp -= damage;
            if (monster.Hp < 0) monster.Hp = 0;
            _view.WriteLine($"{monster.Name} termina con HP:{monster.Hp}/{monster.OriginalHp}");
        }
    }

    private int CalculateDamage(object attacker, int modifier)
    {
        int baseStat = attacker switch
        {
            Samurai samurai => modifier == 54 ? samurai.Str : samurai.Skl,
            Monster monster => modifier == 54 ? monster.Str : monster.Skl,
            _ => 0
        };
        return Convert.ToInt32(Math.Floor(baseStat * modifier * 0.0114));
    }

    private object GetTarget(Team enemyTeam, int targetIndex)
    {
        return targetIndex switch
        {
            1 => enemyTeam.Samurai,
            var idx when idx > 1 && idx <= enemyTeam.Units.Count + 1 => enemyTeam.Units[targetIndex - 2],
            _ => null
        };
    }

    private (int, List<string>) UseSkill(object attacker)
    {
        var (unitName, abilities, currentMp) = GetUnitSkillInfo(attacker);
        _view.WriteLine($"Seleccione una habilidad para que {unitName} use");
        List<string> affordableAbilities = GetAffordableAbilities(abilities, currentMp);
        _view.WriteLine($"{affordableAbilities.Count + 1}-Cancelar");
        int selectedOption = int.Parse(_view.ReadLine());
        return MapSelectedSkill(selectedOption, affordableAbilities, abilities);
    }

    private (string name, List<string> abilities, int currentMp) GetUnitSkillInfo(object attacker)
    {
        return attacker switch
        {
            Samurai samurai => (samurai.Name, samurai.Abilities, samurai.Mp),
            Monster monster => (monster.Name, monster.Abilities, monster.Mp),
            _ => (string.Empty, new List<string>(), 0)
        };
    }

    private List<string> GetAffordableAbilities(List<string> abilities, int currentMp)
    {
        List<string> affordableAbilities = new List<string>();
        for (int abilityIndex = 0; abilityIndex < abilities.Count; abilityIndex++)
        {
            string ability = abilities[abilityIndex];
            int abilityCost = _skillsManager.GetSkillCost(ability);

            if (abilityCost <= currentMp)
            {
                affordableAbilities.Add(ability);
                _view.WriteLine($"{affordableAbilities.Count}-{ability} MP:{abilityCost}");
            }
        }
        return affordableAbilities;
    }

    private (int, List<string>) MapSelectedSkill(int selectedOption, List<string> affordableAbilities, List<string> abilities)
    {
        if (selectedOption > 0 && selectedOption <= affordableAbilities.Count)
        {
            string selectedAbility = affordableAbilities[selectedOption - 1];
            int originalIndex = abilities.IndexOf(selectedAbility);
            return (originalIndex + 1, abilities);
        }
        return (affordableAbilities.Count + 1, abilities);
    }

    private void PrintTurnsUsed(Team currentTeam)
    {
        _view.WriteLine($"Se han consumido 1 Full Turn(s) y 0 Blinking Turn(s)");
        _view.WriteLine("Se han obtenido 0 Blinking Turn(s)");
    }

    private void DisplayWinner(Team teamPlayer1, Team teamPlayer2)
    {
        if (_winnerTeam == 1)
            _view.WriteLine($"Ganador: {teamPlayer1.Samurai.Name} ({teamPlayer1.Player})");
        else
            _view.WriteLine($"Ganador: {teamPlayer2.Samurai.Name} ({teamPlayer2.Player})");
    }
}