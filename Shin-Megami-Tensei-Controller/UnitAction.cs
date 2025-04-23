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
        ProcessAction(currentUnit, selectedAction);
    }
    
    private int GetUnitAction(object currentUnit)
    {
        if (currentUnit is Samurai samurai)
        {
            return _gameUi.GetSamuraiActionOptions(samurai);
        }
        else
        {
            return _gameUi.GetMonsterActionOptions((Monster)currentUnit);
        }
    }
    
    private void ProcessAction(object currentUnit, int action)
    {
        int actionCode = currentUnit is Monster ? action + _actionConstants.MonsterOffset : action;
        bool actionCancelled = false;
        if (actionCode != ActionConstants.PassTurnAction && actionCode != ActionConstants.PassTurnActionMonster)
            _gameUi.PrintLine();
        int initialFullTurns = _currentTeam.FullTurns;
        int initialBlinkingTurns = _currentTeam.BlinkingTurns;

        switch (actionCode)
        {
            case ActionConstants.AttackAction:
            case ActionConstants.ShootAction:
            case ActionConstants.AttackActionMonster:
                actionCancelled = ProcessAttackAction(currentUnit, action);
                break;

            case ActionConstants.SkillAction:
            case ActionConstants.SkillActionMonster:
                actionCancelled = ProcessSkillAction(currentUnit);
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

        
        if (!actionCancelled && !ShouldEndGame)
        {
            
            int fullTurnsUsed = _currentTeam.FullTurns - initialFullTurns;
            int blinkingTurnsUsed = initialBlinkingTurns - _currentTeam.BlinkingTurns;
            if (blinkingTurnsUsed < 0) blinkingTurnsUsed = 0;
            
            int blinkingTurnsGained = (_currentTeam.BlinkingTurns - initialBlinkingTurns) + blinkingTurnsUsed;
            
            // Si blinkingTurnsGained es negativo, lo ajustamos a 0 (no se ganaron turnos)
            if (blinkingTurnsGained < 0) blinkingTurnsGained = 0;
            _gameUi.PrintTurnsUsed(_currentTeam, fullTurnsUsed, blinkingTurnsUsed, blinkingTurnsGained);
            
        }
    }

    private bool ProcessAttackAction(object currentUnit, int action)
    {
        int targetIndex = _targetSelector.ChooseUnitToAttack(currentUnit);

        if (targetIndex == ActionConstants.CancelTargetSelection)
        {
            _gameUi.PrintLine();
            ExecuteUnitTurn(); // Vuelve al menú de selección de acciones
            return true; // Indica que la acción fue cancelada
        }

        _gameUi.PrintLine();
        object targetUnit = _targetSelector.GetTarget(targetIndex);

        string attackType = (action == ActionConstants.AttackAction) ? "Phys" : "Gun";
        string affinity = GetTargetAffinity(targetUnit, attackType);

        if (action == ActionConstants.AttackAction)
            _battleSystem.Attack(currentUnit, targetUnit);
        else
            _battleSystem.Shoot(currentUnit, targetUnit);

        // Consume turnos según la afinidad
        ConsumeSkillTurns(affinity);
    
        _currentTeam.RotateOrderList();
        return false; // Indica que la acción fue completada
    }
    
    private string GetTargetAffinity(object target, string attackType)
    {
        if (target is Samurai samurai)
            return samurai.Affinities.GetAffinity(attackType);
        else if (target is Monster monster)
            return monster.Affinities.GetAffinity(attackType);
    
        return "-"; // Afinidad neutral por defecto
    }

    private bool ProcessSkillAction(object currentUnit)
    {
        var skillInfo = GetSkillSelection(currentUnit);
        _gameUi.PrintLine();
        if (skillInfo.SelectedIndex > skillInfo.Abilities.Count)
        {
            ExecuteUnitTurn();
            return true; // Acción cancelada
        }

        string selectedSkillName = skillInfo.Abilities[skillInfo.SelectedIndex - 1];
        var skillData = _skillsManager.GetSkillByName(selectedSkillName);

        bool isOffensiveSkill = IsOffensiveSkill(skillData.type);

        if (isOffensiveSkill)
        {
            int targetIndex = _targetSelector.ChooseUnitToAttack(currentUnit);
            _gameUi.PrintLine();
            if (targetIndex == ActionConstants.CancelTargetSelection)
            {
                ExecuteUnitTurn();
                return true; // Acción cancelada
            }

            object targetUnit = _targetSelector.GetTarget(targetIndex);
            ConsumeMP(currentUnit, skillData.cost);

            // Aplica la habilidad ofensiva pasando el contador de habilidades del equipo actual
            string affinity = _battleSystem.ApplyOffensiveSkill(currentUnit, targetUnit, skillData, _currentTeam.UsedSkillsCount);

            // Incrementar el contador de habilidades usadas después de usar la habilidad
            _currentTeam.IncrementUsedSkillsCount();

            // Consume turnos según la afinidad
            ConsumeSkillTurns(affinity);
        }
        else
        {
            _gameUi.WriteLine("Las habilidades de apoyo no están implementadas aún.");
            ExecuteUnitTurn();
            return true; // Acción cancelada
        }

        _currentTeam.RotateOrderList();
        return false; // Acción completada
    }

    private bool IsOffensiveSkill(string skillType)
    {
        return skillType == "Phys" || skillType == "Gun" || 
               skillType == "Fire" || skillType == "Ice" || 
               skillType == "Elec" || skillType == "Force";
    }

    private void ConsumeMP(object unit, int mpCost)
    {
        if (unit is Samurai samurai)
        {
            samurai.Mp -= mpCost;
        }
        else if (unit is Monster monster)
        {
            monster.Mp -= mpCost;
        }
    }
    
    private void ProcessInvokeAction()
    {
        // Gasta 1 Blinking Turn, si no hay, paga 1 Full Turn y obtén 1 Blinking Turn adicional
        if (_currentTeam.BlinkingTurns > 0)
            _currentTeam.ConsumeBlinkingTurn();
        else
        {
            _currentTeam.ConsumeFullTurn();
            _currentTeam.AddBlinkingTurn();
        }

        _currentTeam.RotateOrderList();
    }

    private void ProcessPassTurnAction()
    {
        if (_currentTeam.BlinkingTurns > 0)
            _currentTeam.ConsumeBlinkingTurn();
        else
        {
            _currentTeam.ConsumeFullTurn();
            _currentTeam.AddBlinkingTurn();
        }

        _currentTeam.RotateOrderList();
    }

    private void ProcessSurrenderAction()
    {
        _gameUi.WriteLine($"{_currentTeam.Samurai.Name} ({_currentTeam.Player}) se rinde");
        ShouldEndGame = true;
    }
    
    private SkillSelectionResult GetSkillSelection(object attacker)
    {
        var unitSkillInfo = CreateUnitSkillInfo(attacker);
        _gameUi.WriteLine($"Seleccione una habilidad para que {unitSkillInfo.Name} use");
        
        List<string> affordableAbilities = GetAffordableAbilities(unitSkillInfo);
        _gameUi.WriteLine($"{affordableAbilities.Count + 1}-Cancelar");

        int selectedOption = int.Parse(_gameUi.ReadLine());
        if (selectedOption > affordableAbilities.Count)
            return new SkillSelectionResult(unitSkillInfo.Abilities.Count + 1, unitSkillInfo.Abilities);

        if (selectedOption > 0 && selectedOption <= affordableAbilities.Count)
        {
            string selectedAbility = affordableAbilities[selectedOption - 1];
            int originalIndex = unitSkillInfo.Abilities.IndexOf(selectedAbility);
            return new SkillSelectionResult(originalIndex + 1, unitSkillInfo.Abilities);
        }
        return new SkillSelectionResult(unitSkillInfo.Abilities.Count + 1, unitSkillInfo.Abilities);
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
    
    private void ConsumeSkillTurns(string affinity)
    {
        switch (affinity)
        {
            case "Rp": // Repel
            case "Dr": // Drain
                ConsumeAllTurns();
                break;

            case "Nu": // Null
                // Consume 2 Blinking Turns, o los que haya y el resto como Full Turns
                ConsumeMultipleBlinkingTurns(2);
                break;

            case "Miss": // Miss
                // Consume 1 Blinking Turn, si no hay, paga 1 Full Turn
                if (_currentTeam.BlinkingTurns > 0)
                    _currentTeam.ConsumeBlinkingTurn();
                else
                    _currentTeam.ConsumeFullTurn();
                break;

            case "Wk": // Weak
                // Consume 1 Full Turn y otorga 1 Blinking Turn extra si hay Full Turns disponibles
                // Si no hay Full Turns, consume un Blinking Turn
                if (_currentTeam.FullTurns < _currentTeam.MaxFullTurns)
                {
                    _currentTeam.ConsumeFullTurn();
                    _currentTeam.AddBlinkingTurn();
                }
                else if (_currentTeam.BlinkingTurns > 0)
                    _currentTeam.ConsumeBlinkingTurn();
                else
                    _currentTeam.ConsumeFullTurn(); // No debería ocurrir normalmente, pero como fallback
                break;

            default: // Neutral o Resist (Rs)
                // Consume 1 Blinking Turn, si no hay, paga 1 Full Turn
                if (_currentTeam.BlinkingTurns > 0)
                    _currentTeam.ConsumeBlinkingTurn();
                else
                    _currentTeam.ConsumeFullTurn();
                break;
        }
    }

    private void ConsumeAllTurns()
    {
        int blinkingTurnsToConsume = _currentTeam.BlinkingTurns;
        for (int i = 0; i < blinkingTurnsToConsume; i++)
        {
            _currentTeam.ConsumeBlinkingTurn();
        }

        while (_currentTeam.FullTurns < _currentTeam.MaxFullTurns)
        {
            _currentTeam.ConsumeFullTurn();
        }
    }

    private void ConsumeMultipleBlinkingTurns(int count)
    {
        // Consume hasta 'count' Blinking Turns
        int blinkingTurnsAvailable = _currentTeam.BlinkingTurns;
    
        // Consume primero los Blinking Turns disponibles
        for (int i = 0; i < Math.Min(count, blinkingTurnsAvailable); i++)
        {
            _currentTeam.ConsumeBlinkingTurn();
        }
    
        // Si necesitas más turnos, usa Full Turns para los restantes
        int fullTurnsNeeded = count - blinkingTurnsAvailable;
        int fullTurnsAvailable = _currentTeam.MaxFullTurns - _currentTeam.FullTurns;
    
        // Consume el mínimo entre los turnos necesarios y los disponibles
        int fullTurnsToConsume = Math.Min(fullTurnsNeeded, fullTurnsAvailable);
    
        for (int i = 0; i < fullTurnsToConsume; i++)
        {
            _currentTeam.ConsumeFullTurn();
        }
    }
}