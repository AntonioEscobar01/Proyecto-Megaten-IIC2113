using Shin_Megami_Tensei_View;
namespace Shin_Megami_Tensei;

public class Game
{
    private readonly View _view;
    private readonly string _teamsFolder;
    private int _currentTurn;
    private SkillsManager _skillsManager;
    private Boolean _isGameOver;
    private int _winnerTeam;
    private Team _teamPlayer1;
    private Team _teamPlayer2;
    
    // Constantes para reemplazar valores mágicos
    private const int ATTACK_ACTION = 1;
    private const int SHOOT_ACTION = 2;
    private const int SKILL_ACTION = 3;
    private const int INVOKE_ACTION = 4;
    private const int PASS_TURN_ACTION = 5;
    private const int SURRENDER_ACTION = 6;
    
    private const int MONSTER_ACTION_OFFSET = 10;
    private const int CANCEL_TARGET_SELECTION = 5;
    
    private const int ATTACK_DAMAGE_MODIFIER = 54;
    private const int SHOOT_DAMAGE_MODIFIER = 80;

    public Game(View view, string teamsFolderInitializer)
    {
        _view = view;
        _teamsFolder = teamsFolderInitializer;
        _currentTurn = 1;
        _isGameOver = false;
        _winnerTeam = 0;
    }

    public void Play()
    {
        var teamFiles = LoadTeamFiles();
        _skillsManager = new SkillsManager();
        
        var teamsInitialized = InitializeTeams(teamFiles);
        if (!teamsInitialized)
            return;
            
        PrintLine();
        PrintPlayerRound();
        PrintLine();
        RunGameLoop();
        DisplayWinner();
    }

    private string[] LoadTeamFiles()
    {
        _view.WriteLine("Elige un archivo para cargar los equipos");
        var teamFiles = Directory.GetFiles(_teamsFolder);
        ShowFiles(teamFiles);
        return teamFiles;
    }

    private bool InitializeTeams(string[] teamFiles)
    {
        int chosenFileIndex = int.Parse(_view.ReadLine());
        string chosenFile = teamFiles[chosenFileIndex];
        var teamsInfo = File.ReadAllLines(chosenFile);
        
        var (unitsPlayer1Names, unitsPlayer2Names) = ExtractTeamUnitNames(teamsInfo);
        
        _teamPlayer1 = new Team("J1");
        _teamPlayer2 = new Team("J2");
        
        bool isTeamOneValid = _teamPlayer1.IsValidTeam(unitsPlayer1Names);
        bool isTeamTwoValid = _teamPlayer2.IsValidTeam(unitsPlayer2Names);
        
        InitializeTeamOrders();
        
        if (!isTeamOneValid || !isTeamTwoValid)
        {
            _view.WriteLine("Archivo de equipos inválido");
            return false;
        }
        return true;
    }
    
    private void InitializeTeamOrders()
    {
        _teamPlayer1.InitializeOrderList();
        _teamPlayer2.InitializeOrderList();
        _teamPlayer1.SetMaxFullTurns();
        _teamPlayer2.SetMaxFullTurns();
    }

    private void RunGameLoop()
    {
        while (!_isGameOver)
        {
            var currentTeam = GetCurrentTeam();
            currentTeam.SetMaxFullTurns();

            DisplayGameState();
            ProcessTurn();

            CheckGameOver();
            HandleTurnTransition(currentTeam);

            _teamPlayer1.RemoveDeadUnits();
            _teamPlayer2.RemoveDeadUnits();
        }
    }

    private void DisplayGameState()
    {
        PrintTeamsState();
        PrintLine();
        PrintTurnInfo();
        PrintLine();
        ShowTeamOrder();
        PrintLine();
    }

    private void ProcessTurn()
    {
        ExecuteGameFlow();
        PrintLine();
        if (!_isGameOver)
        {
            PrintTurnsUsed(GetCurrentTeam());
            PrintLine();
        }
    }

    private void CheckGameOver()
    {
        if (_teamPlayer1.AreAllUnitsDead())
        {
            _isGameOver = true;
            _winnerTeam = 2;
        }
        if (_teamPlayer2.AreAllUnitsDead())
        {
            _isGameOver = true;
            _winnerTeam = 1;
        }
    }

    private void HandleTurnTransition(Team currentTeam)
    {
        if (currentTeam.HasCompletedAllTurns() && !_isGameOver)
        {
            _currentTurn++;
            PrintPlayerRound();
            PrintLine();
            currentTeam.ResetTurns();
        }
    }

    private void ExecuteGameFlow()
    {
        var currentTeam = GetCurrentTeam();
        var currentUnit = currentTeam.OrderList[0];
        int selectedAction = GetUnitAction(currentUnit);
        PrintLine();
        ProcessAction(currentTeam, currentUnit, selectedAction);
    }

    private int GetUnitAction(object currentUnit)
    {
        if (currentUnit is Samurai samurai)
        {
            return GetSamuraiActionOptions(samurai);
        }
        else
        {
            return GetMonsterActionOptions((Monster)currentUnit);
        }
    }

    private void ProcessAction(Team currentTeam, object currentUnit, int action)
    {
        int actionCode = currentUnit is Monster ? action + MONSTER_ACTION_OFFSET : action;
        switch (actionCode)
        {
            case ATTACK_ACTION: case SHOOT_ACTION: case ATTACK_ACTION + MONSTER_ACTION_OFFSET: 
                ProcessAttackAction(currentTeam, currentUnit, action); break;
            case SKILL_ACTION: case SKILL_ACTION + MONSTER_ACTION_OFFSET:
                ProcessSkillAction(currentUnit); break;
            case INVOKE_ACTION: case INVOKE_ACTION + MONSTER_ACTION_OFFSET:
                ProcessInvokeAction(); break;
            case PASS_TURN_ACTION: case PASS_TURN_ACTION + MONSTER_ACTION_OFFSET:
                ProcessPassTurnAction(currentTeam); break;
            case SURRENDER_ACTION:
                ProcessSurrenderAction(currentTeam); break;
        }
    }

    private void ProcessAttackAction(Team currentTeam, object currentUnit, int action)
    {
        var enemyTeam = GetEnemyTeam();
        int targetIndex = ChooseUnitToAttack(enemyTeam, currentUnit);
        if (targetIndex == CANCEL_TARGET_SELECTION)
        {
            PrintLine();
            ExecuteGameFlow();
            return;
        }
        PrintLine();
        object targetUnit = GetTarget(enemyTeam, targetIndex);

        if (action == ATTACK_ACTION)
            Attack(currentUnit, targetUnit);
        else
            Shoot(currentUnit, targetUnit);
        currentTeam.RotateOrderList();
        currentTeam.CompleteTurn();
    }

    private void ProcessSkillAction(object currentUnit)
    {
        var skillInfo = GetSkillSelection(currentUnit);
        PrintLine();
        ExecuteGameFlow();
    }

    private void ProcessInvokeAction()
    {
    }

    private void ProcessPassTurnAction(Team currentTeam)
    {
        currentTeam.RotateOrderList();
        currentTeam.CompleteTurn();
    }

    private void ProcessSurrenderAction(Team currentTeam)
    {
        _view.WriteLine($"{currentTeam.Samurai.Name} ({currentTeam.Player}) se rinde");
        _isGameOver = true;
        _winnerTeam = (_currentTurn % 2 != 0) ? 2 : 1;
    }

    private Team GetCurrentTeam()
    {
        return (_currentTurn % 2 != 0) ? _teamPlayer1 : _teamPlayer2;
    }

    private Team GetEnemyTeam()
    {
        return (_currentTurn % 2 != 0) ? _teamPlayer2 : _teamPlayer1;
    }

    private void PrintLine()
    {
        _view.WriteLine("----------------------------------------");
    }

    private void PrintPlayerRound()
    {
        Team currentTeam = GetCurrentTeam();
        string playerInfo = $"Ronda de {currentTeam.Samurai.Name} ({currentTeam.Player})";
        _view.WriteLine(playerInfo);
    }

    private void PrintTurnInfo()
    {
        Team currentTeam = GetCurrentTeam();
        _view.WriteLine($"Full Turns: {currentTeam.MaxFullTurns - currentTeam.FullTurns}");
        _view.WriteLine($"Blinking Turns: {currentTeam.BlinkingTurns}");
    }

    private void ShowTeamOrder()
    {
        Team currentTeam = GetCurrentTeam();

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

    private (List<string> unitsPlayer1, List<string> unitsPlayer2) ExtractTeamUnitNames(string[] teamsInfo)
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

    private void PrintTeamsState()
    {
        PrintTeamState(_teamPlayer1);
        PrintTeamState(_teamPlayer2);
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

    private int GetSamuraiActionOptions(Samurai samurai)
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

    private int GetMonsterActionOptions(Monster monster)
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

        return GetTargetSelectionResult(availableTargets, enemyTeam);
    }

    private List<object> GetAvailableTargets(Team enemyTeam)
    {
        List<object> availableTargets = new List<object>();
        AddSamuraiIfAvailable(enemyTeam, availableTargets);
        AddAvailableMonsters(enemyTeam, availableTargets);
        return availableTargets;
    }
    
    private void AddSamuraiIfAvailable(Team enemyTeam, List<object> availableTargets)
    {
        if (enemyTeam.Samurai != null && !enemyTeam.Samurai.IsDead())
        {
            availableTargets.Add(enemyTeam.Samurai);
        }
    }
    
    private void AddAvailableMonsters(Team enemyTeam, List<object> availableTargets)
    {
        int maxVisibleMonsters = Math.Min(enemyTeam.Units.Count, 3);
        for (int monsterIndex = 0; monsterIndex < maxVisibleMonsters; monsterIndex++)
        {
            var monster = enemyTeam.Units[monsterIndex];
            if (!monster.IsDead())
            {
                availableTargets.Add(monster);
            }
        }
    }

    private void DisplayTargets(List<object> availableTargets)
    {
        for (int targetIndex = 0; targetIndex < availableTargets.Count; targetIndex++)
        {
            DisplayTarget(availableTargets[targetIndex], targetIndex);
        }
        _view.WriteLine($"{availableTargets.Count + 1}-Cancelar");
    }
    
    private void DisplayTarget(object target, int targetIndex)
    {
        if (target is Samurai samurai)
        {
            _view.WriteLine($"{targetIndex+1}-{samurai.Name} HP:{samurai.Hp}/{samurai.OriginalHp} MP:{samurai.Mp}/{samurai.OriginalMp}");
        }
        else if (target is Monster monster)
        {
            _view.WriteLine($"{targetIndex+1}-{monster.Name} HP:{monster.Hp}/{monster.OriginalHp} MP:{monster.Mp}/{monster.OriginalMp}");
        }
    }

    private int GetTargetSelectionResult(List<object> availableTargets, Team enemyTeam)
    {
        int selection = int.Parse(_view.ReadLine());

        if (selection == availableTargets.Count + 1)
            return CANCEL_TARGET_SELECTION;
        if (selection > 0 && selection <= availableTargets.Count)
        {
            var selectedTarget = availableTargets[selection - 1];
            if (selectedTarget is Samurai)
                return 1;
            else
                return enemyTeam.Units.IndexOf((Monster)selectedTarget) + 2;
        }
        return CANCEL_TARGET_SELECTION;
    }

    private void Attack(object attacker, object target)
    {
        int damage = CalculateDamage(attacker, ATTACK_DAMAGE_MODIFIER);
        ApplyDamage(attacker, target, damage, "ataca");
    }

    private void Shoot(object attacker, object target)
    {
        int damage = CalculateDamage(attacker, SHOOT_DAMAGE_MODIFIER);
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
            UpdateUnitHealth(samurai, damage);
        }
        else if (target is Monster monster)
        {
            UpdateUnitHealth(monster, damage);
        }
    }
    
    private void UpdateUnitHealth(dynamic unit, int damage)
    {
        unit.Hp -= damage;
        if (unit.Hp < 0) unit.Hp = 0;
        _view.WriteLine($"{unit.Name} termina con HP:{unit.Hp}/{unit.OriginalHp}");
    }

    private int CalculateDamage(object attacker, int modifier)
    {
        int baseStat = attacker switch
        {
            Samurai samurai => modifier == ATTACK_DAMAGE_MODIFIER ? samurai.Str : samurai.Skl,
            Monster monster => modifier == ATTACK_DAMAGE_MODIFIER ? monster.Str : monster.Skl,
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

    private UnitSkillInfo GetUnitSkillInfo(object attacker)
    {
        if (attacker is Samurai samurai)
        {
            return new UnitSkillInfo(samurai.Name, samurai.Abilities, samurai.Mp);
        }
        else if (attacker is Monster monster)
        {
            return new UnitSkillInfo(monster.Name, monster.Abilities, monster.Mp);
        }
        return new UnitSkillInfo(string.Empty, new List<string>(), 0);
    }

    private SkillSelectionResult GetSkillSelection(object attacker)
    {
        var skillInfo = GetUnitSkillInfo(attacker);
        _view.WriteLine($"Seleccione una habilidad para que {skillInfo.Name} use");
        
        List<string> affordableAbilities = GetAffordableAbilities(skillInfo);
        _view.WriteLine($"{affordableAbilities.Count + 1}-Cancelar");
        
        int selectedOption = int.Parse(_view.ReadLine());
        return MapSkillSelectionToResult(selectedOption, affordableAbilities, skillInfo.Abilities);
    }

    private List<string> GetAffordableAbilities(UnitSkillInfo skillInfo)
    {
        List<string> affordableAbilities = new List<string>();
        for (int abilityIndex = 0; abilityIndex < skillInfo.Abilities.Count; abilityIndex++)
        {
            string ability = skillInfo.Abilities[abilityIndex];
            int abilityCost = _skillsManager.GetSkillCost(ability);

            if (abilityCost <= skillInfo.CurrentMp)
            {
                affordableAbilities.Add(ability);
                _view.WriteLine($"{affordableAbilities.Count}-{ability} MP:{abilityCost}");
            }
        }
        return affordableAbilities;
    }

    private SkillSelectionResult MapSkillSelectionToResult(int selectedOption, List<string> affordableAbilities, List<string> allAbilities)
    {
        if (selectedOption > 0 && selectedOption <= affordableAbilities.Count)
        {
            string selectedAbility = affordableAbilities[selectedOption - 1];
            int originalIndex = allAbilities.IndexOf(selectedAbility);
            return new SkillSelectionResult(originalIndex + 1, allAbilities);
        }
        return new SkillSelectionResult(affordableAbilities.Count + 1, allAbilities);
    }

    private void PrintTurnsUsed(Team currentTeam)
    {
        _view.WriteLine($"Se han consumido 1 Full Turn(s) y 0 Blinking Turn(s)");
        _view.WriteLine("Se han obtenido 0 Blinking Turn(s)");
    }

    private void DisplayWinner()
    {
        if (_winnerTeam == 1)
            _view.WriteLine($"Ganador: {_teamPlayer1.Samurai.Name} ({_teamPlayer1.Player})");
        else
            _view.WriteLine($"Ganador: {_teamPlayer2.Samurai.Name} ({_teamPlayer2.Player})");
    }
}



