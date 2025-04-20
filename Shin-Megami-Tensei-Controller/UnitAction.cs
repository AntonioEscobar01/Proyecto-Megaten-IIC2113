namespace Shin_Megami_Tensei;

public class UnitAction
{
    private readonly Team _currentTeam;
    private readonly Team _enemyTeam;
    private readonly GameUi _gameUi;
    private readonly SkillsManager _skillsManager;
    private readonly BattleSystem _battleSystem;
    private readonly TargetSelector _targetSelector;
    private readonly ActionConstants _actionConstants;
    
    public bool ShouldEndGame { get; private set; }

    public UnitAction(Team currentTeam, Team enemyTeam, GameUi gameUi, 
                      SkillsManager skillsManager, BattleSystem battleSystem)
    {
        _currentTeam = currentTeam;
        _enemyTeam = enemyTeam;
        _gameUi = gameUi;
        _skillsManager = skillsManager;
        _battleSystem = battleSystem;
        _targetSelector = new TargetSelector(gameUi, enemyTeam);
        _actionConstants = new ActionConstants();
        ShouldEndGame = false;
    }

    public void ExecuteUnitTurn()
    {
        var currentUnit = _currentTeam.OrderList[0];
        int selectedAction = GetUnitAction(currentUnit);
        _gameUi.PrintLine();
        ProcessAction(currentUnit, selectedAction);
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

    private int GetSamuraiActionOptions(Samurai samurai)
    {
        _gameUi.WriteLine($"Seleccione una acción para {samurai.Name}");
        _gameUi.WriteLine("1: Atacar");
        _gameUi.WriteLine("2: Disparar");
        _gameUi.WriteLine("3: Usar Habilidad");
        _gameUi.WriteLine("4: Invocar");
        _gameUi.WriteLine("5: Pasar Turno");
        _gameUi.WriteLine("6: Rendirse");
        return int.Parse(_gameUi.ReadLine());
    }

    private int GetMonsterActionOptions(Monster monster)
    {
        _gameUi.WriteLine($"Seleccione una acción para {monster.Name}");
        _gameUi.WriteLine("1: Atacar");
        _gameUi.WriteLine("2: Usar Habilidad");
        _gameUi.WriteLine("3: Invocar");
        _gameUi.WriteLine("4: Pasar Turno");
        return int.Parse(_gameUi.ReadLine());
    }

    private void ProcessAction(object currentUnit, int action)
    {
        int actionCode = currentUnit is Monster ? action + _actionConstants.MonsterOffset : action;        
        switch (actionCode)
        {
            case ActionConstants.AttackAction:
            case ActionConstants.ShootAction:
            case ActionConstants.AttackActionMonster:
                ProcessAttackAction(currentUnit, action);
                break;
                
            case ActionConstants.SkillAction:
            case ActionConstants.SkillActionMonster:
                ProcessSkillAction(currentUnit);
                break;
                
            case ActionConstants.InvokeAction:
            case ActionConstants.InvokeActionMonster:
                ProcessInvokeAction();
                break;
                
            case ActionConstants.PassTurnAction:
            case ActionConstants.PassTurnActionMonster:
                ProcessPassTurnAction();
                break;
                
            case ActionConstants.SurrenderAction:
                ProcessSurrenderAction();
                break;
        }
    }

    private void ProcessAttackAction(object currentUnit, int action)
    {
        int targetIndex = _targetSelector.ChooseUnitToAttack(currentUnit);
        
        if (targetIndex == ActionConstants.CancelTargetSelection)
        {
            _gameUi.PrintLine();
            ExecuteUnitTurn();
            return;
        }
        
        _gameUi.PrintLine();
        object targetUnit = _targetSelector.GetTarget(targetIndex);

        if (action == ActionConstants.AttackAction)
            _battleSystem.Attack(currentUnit, targetUnit);
        else
            _battleSystem.Shoot(currentUnit, targetUnit);
            
        CompleteTurn();
    }

    private void ProcessSkillAction(object currentUnit)
    {
        var skillInfo = GetSkillSelection(currentUnit);
        _gameUi.PrintLine();
        ExecuteUnitTurn();
    }

        private void ProcessInvokeAction()
    {
        object currentUnit = _currentTeam.OrderList[0];
        bool isSamurai = currentUnit is Samurai;

        // Obtener monstruos disponibles para invocar
        var availableMonsters = GetAvailableMonsters();

        if (availableMonsters.Count == 0)
        {
            _gameUi.PrintLine();
            _gameUi.WriteLine("No hay monstruos disponibles para invocar");
            _gameUi.PrintLine();
            ExecuteUnitTurn();
            return;
        }

        // Mostrar menú de selección de monstruos
        _gameUi.PrintLine();
        _gameUi.WriteLine("Seleccione un monstruo para invocar");
        DisplayAvailableMonsters(availableMonsters);
        _gameUi.WriteLine($"{availableMonsters.Count + 1}- Cancelar");

        int selection = int.Parse(_gameUi.ReadLine());
        
        // Verificar si se seleccionó cancelar
        if (selection > availableMonsters.Count || selection <= 0)
        {
            _gameUi.PrintLine();
            ExecuteUnitTurn();
            return;
        }

        Monster selectedMonster = availableMonsters[selection - 1];

        if (isSamurai)
        {
            ProcessSamuraiInvoke(selectedMonster);
        }
        else
        {
            ProcessMonsterInvoke(selectedMonster);
        }
    }

    private List<Monster> GetAvailableMonsters()
    {
        // Obtener monstruos que no están en los primeros 3 puestos y no están muertos
        List<Monster> availableMonsters = new List<Monster>();
        
        for (int i = 3; i < _currentTeam.Units.Count; i++)
        {
            if (i < _currentTeam.Units.Count && !_currentTeam.Units[i].IsDead())
            {
                availableMonsters.Add(_currentTeam.Units[i]);
            }
        }
        
        return availableMonsters;
    }

    private void DisplayAvailableMonsters(List<Monster> monsters)
    {
        for (int i = 0; i < monsters.Count; i++)
        {
            Monster monster = monsters[i];
            _gameUi.WriteLine($"{i + 1}- {monster.Name} HP:{monster.Hp}/{monster.OriginalHp} MP:{monster.Mp}/{monster.OriginalMp}");
        }
    }

    private void ProcessSamuraiInvoke(Monster selectedMonster)
    {
        _gameUi.PrintLine();
        _gameUi.WriteLine("Seleccione una posición para invocar");
        
        // Mostrar posiciones disponibles (puestos 2, 3, 4)
        List<int> positions = new List<int>();
        for (int i = 0; i < 3; i++) // Posiciones 2, 3, 4
        {
            string positionInfo;
            if (i < _currentTeam.Units.Count && !_currentTeam.Units[i].IsDead())
            {
                Monster monster = _currentTeam.Units[i];
                positionInfo = $"{monster.Name} HP:{monster.Hp}/{monster.OriginalHp} MP:{monster.Mp}/{monster.OriginalMp} (Puesto {i + 2})";
            }
            else
            {
                positionInfo = $"Vacío (Puesto {i + 2})";
            }
            
            _gameUi.WriteLine($"{i + 1}- {positionInfo}");
            positions.Add(i + 2);
        }
        
        _gameUi.WriteLine($"{positions.Count + 1}- Cancelar");
        
        int positionSelection = int.Parse(_gameUi.ReadLine());
        
        // Verificar si se seleccionó cancelar
        if (positionSelection > positions.Count || positionSelection <= 0)
        {
            _gameUi.PrintLine();
            ExecuteUnitTurn();
            return;
        }
        
        int selectedPosition = positionSelection - 1; // Índice real en Units
        
        // Realizar la invocación
        PlaceMonsterAtPosition(selectedMonster, selectedPosition);
        
        _gameUi.PrintLine();
        _gameUi.WriteLine($"{selectedMonster.Name} ha sido invocado");
        _gameUi.PrintLine();
        _gameUi.WriteLine("Se han consumido 1 Full Turn(s) y 0 Blinking Turn(s)");
        _gameUi.WriteLine("Se han obtenido 1 Blinking Turn(s)");
        
        _currentTeam.AddBlinkingTurn();
        CompleteTurn();
    }

    private void ProcessMonsterInvoke(Monster selectedMonster)
    {
        Monster currentMonster = _currentTeam.OrderList[0] as Monster;
        int currentPosition = _currentTeam.Units.IndexOf(currentMonster);
        
        // Intercambiar el monstruo actual por el seleccionado
        _currentTeam.Units[currentPosition] = selectedMonster;
        _currentTeam.Units.Remove(selectedMonster);
        
        _gameUi.PrintLine();
        _gameUi.WriteLine($"{selectedMonster.Name} ha sido invocado");
        _gameUi.PrintLine();
        _gameUi.WriteLine("Se han consumido 0 Full Turn(s) y 1 Blinking Turn(s)");
        _gameUi.WriteLine("Se han obtenido 0 Blinking Turn(s)");
        
        // Actualizar la lista de orden
        _currentTeam.InitializeOrderList();
        CompleteTurn();
    }

    private void PlaceMonsterAtPosition(Monster monster, int position)
    {
        // Eliminar el monstruo de su posición actual
        _currentTeam.Units.Remove(monster);
        
        // Asegurar que hay suficientes posiciones
        while (_currentTeam.Units.Count <= position)
        {
            _currentTeam.Units.Add(null);
        }
        
        // Colocar el monstruo en la posición seleccionada
        _currentTeam.Units[position] = monster;
        
        // Actualizar la lista de orden
        _currentTeam.InitializeOrderList();
    }
    private void ProcessPassTurnAction()
    {
        CompleteTurn();
    }

    private void ProcessSurrenderAction()
    {
        _gameUi.WriteLine($"{_currentTeam.Samurai.Name} ({_currentTeam.Player}) se rinde");
        ShouldEndGame = true;
    }
    
    private void CompleteTurn()
    {
        _currentTeam.RotateOrderList();
        _currentTeam.CompleteTurn();
    }
    
    private SkillSelectionResult GetSkillSelection(object attacker)
    {
        var unitSkillInfo = CreateUnitSkillInfo(attacker);
        _gameUi.WriteLine($"Seleccione una habilidad para que {unitSkillInfo.Name} use");

        List<string> affordableAbilities = GetAffordableAbilities(unitSkillInfo);
        _gameUi.WriteLine($"{affordableAbilities.Count + 1}-Cancelar");

        int selectedOption = int.Parse(_gameUi.ReadLine());
        return MapSkillSelectionToResult(selectedOption, affordableAbilities, unitSkillInfo.Abilities);
    }

    private UnitSkillInfo CreateUnitSkillInfo(object attacker)
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
                _gameUi.WriteLine($"{affordableAbilities.Count}-{ability} MP:{abilityCost}");
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
}