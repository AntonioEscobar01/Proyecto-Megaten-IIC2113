namespace Shin_Megami_Tensei;

public class UnitActionController
{
    private readonly Team _currentTeam;
    private readonly Team _enemyTeam;
    private readonly GameUi _gameUi;
    private readonly SkillsManager _skillsManager;
    private readonly AttackProcessor _attackProcessor;
    private readonly TargetSelectorController _targetSelectorController;
    private readonly ActionConstantsData _actionConstantsData;
    private readonly AllySelectorController _allySelectorController;
    
    public bool ShouldEndGame { get; private set; }

    public UnitActionController(Team currentTeam, Team enemyTeam, GameUi gameUi, 
        SkillsManager skillsManager, AttackProcessor attackProcessor)
    {
        _currentTeam = currentTeam;
        _enemyTeam = enemyTeam;
        _gameUi = gameUi;
        _skillsManager = skillsManager;
        _attackProcessor = attackProcessor;
        _targetSelectorController = new TargetSelectorController(gameUi, enemyTeam);
        _allySelectorController = new AllySelectorController(gameUi, currentTeam);
        _actionConstantsData = new ActionConstantsData();
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
    
    private void ProcessAction(IUnit currentUnit, int action)
    {
        int actionCode = currentUnit is Monster ? action + _actionConstantsData.MonsterOffset : action;
        bool actionCancelled = false;
        if (actionCode != ActionConstantsData.PassTurnAction && actionCode != ActionConstantsData.PassTurnActionMonster)
            _gameUi.PrintLine();
        int initialFullTurns = _currentTeam.FullTurns;
        int initialBlinkingTurns = _currentTeam.BlinkingTurns;

        switch (actionCode)
        {
            case ActionConstantsData.AttackAction:
            case ActionConstantsData.ShootAction:
            case ActionConstantsData.AttackActionMonster:
                actionCancelled = ProcessAttackAction(currentUnit, action);
                break;

            case ActionConstantsData.SkillAction:
            case ActionConstantsData.SkillActionMonster:
                actionCancelled = WasSkillActionCancelled(currentUnit);
                break;

            case ActionConstantsData.InvokeAction:
            case ActionConstantsData.InvokeActionMonster:
                actionCancelled = WasInvokeActionCancelled(currentUnit);
                break;

            case ActionConstantsData.PassTurnAction:
            case ActionConstantsData.PassTurnActionMonster:
                ProcessPassTurnAction();
                break;

            case ActionConstantsData.SurrenderAction:
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
        int targetIndex = _targetSelectorController.ChooseUnitToAttack(currentUnit);

        if (targetIndex == ActionConstantsData.CancelTargetSelection)
        {
            _gameUi.PrintLine();
            ExecuteUnitTurn(); // Vuelve al menú de selección de acciones
            return true; // Indica que la acción fue cancelada
        }

        _gameUi.PrintLine();
        object targetUnit = _targetSelectorController.GetTarget(targetIndex);

        string attackType = (action == ActionConstantsData.AttackAction) ? "Phys" : "Gun";
        string affinity = GetTargetAffinity(targetUnit, attackType);

        if (action == ActionConstantsData.AttackAction)
            _attackProcessor.Attack(currentUnit, targetUnit);
        else
            _attackProcessor.Shoot(currentUnit, targetUnit);

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

    private bool WasSkillActionCancelled(IUnit currentUnit)
    {
        var skillInfo = GetSkillSelection(currentUnit);
        _gameUi.PrintLine();
        if (skillInfo.SelectedIndex > skillInfo.Abilities.Count)
        {
            ExecuteUnitTurn();
            return true;
        }

        string selectedSkillName = skillInfo.Abilities[skillInfo.SelectedIndex - 1];
        var skillData = _skillsManager.GetSkillByName(selectedSkillName);

        if (IsOffensiveSkill(skillData.type))
        {
            return HandleOffensiveSkillExecution(currentUnit, skillData);
        }
        else if (skillData.type == "Heal")
        {
            return WasHealingSkillCancelled(currentUnit, skillData);
        }
        else if(skillData.type == "Special")
        {
            return WasSpecialSkillCancelled(currentUnit, skillData);
        }

        _currentTeam.RotateOrderList();
        return false;
    }

    private bool IsOffensiveSkill(string skillType)
    {
        return skillType == "Phys" || skillType == "Gun" || 
               skillType == "Fire" || skillType == "Ice" || 
               skillType == "Elec" || skillType == "Force";
    }

    private void ConsumeMp(object unit, int mpCost)
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
    
    private bool WasInvokeActionCancelled(IUnit currentUnit)
    {
        bool isSamurai = currentUnit is Samurai;
        List<Monster> availableMonsters = _currentTeam.GetAvailableMonstersForSummon();
        int selectedMonsterIndex = _gameUi.DisplaySummonMenu(availableMonsters);
    
        if (selectedMonsterIndex == availableMonsters.Count + 1)
        {
            _gameUi.PrintLine();
            ExecuteUnitTurn();
            return true;
        }

        Monster selectedMonster = availableMonsters[selectedMonsterIndex - 1];

        if (isSamurai)
        {
            return HandleSamuraiInvocation(selectedMonster);
        }
        else
        {
            HandleMonsterInvocation((Monster)currentUnit, selectedMonster);
            return false;
        }
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
    
    private bool WasHealingSkillCancelled(object currentUnit, SkillData skillData)
    {
        string[] percentageHealSkills = { "Dia", "Diarama", "Diarahan" };
        string[] reviveHealSkills = { "Recarm", "Samarecarm" };
        bool isHealSkill = percentageHealSkills.Contains(skillData.name);
        bool isReviveSkill = reviveHealSkills.Contains(skillData.name);
        bool isInvitationSkill = skillData.name == "Invitation";
    
        if (!isInvitationSkill)
        {
            return HandleRegularHealingSkill(currentUnit, skillData, isHealSkill, isReviveSkill);
        }
        else
        {
            return HandleInvitationSkillExecution(currentUnit, skillData);
        }
    }
    
    private bool WasInvitationSkillCancelled(object currentUnit)
    {
        List<Monster> availableMonsters = GetAllReserveMonsters();
        int monsterSelection = _gameUi.DisplaySummonMenu(availableMonsters);
    
        if (monsterSelection == availableMonsters.Count + 1)
        {
            return true;
        }
    
        Monster selectedMonster = availableMonsters[monsterSelection - 1];
        _gameUi.PrintLine();
    
        int positionSelection = _gameUi.DisplayPositionMenu(_currentTeam);
        if (positionSelection == 4) // Cancelar
            return true;

        ExecuteInvitationSkill(currentUnit, selectedMonster, positionSelection);
        return false;
    }

    private bool WasSpecialSkillCancelled(IUnit currentUnit, SkillData skillData)
    {
        if (skillData.name == "Sabbatma")
        {
            return HandleSabbatmaSkillExecution(currentUnit, skillData);
        }

        return false;
    }

    private bool HandleOffensiveSkillExecution(IUnit currentUnit, SkillData skillData)
    {
        int targetIndex = _targetSelectorController.ChooseUnitToAttack(currentUnit);
        _gameUi.PrintLine();
        
        if (targetIndex == ActionConstantsData.CancelTargetSelection)
        {
            ExecuteUnitTurn();
            return true;
        }

        object targetUnit = _targetSelectorController.GetTarget(targetIndex);
        ExecuteOffensiveSkill(currentUnit, targetUnit, skillData);
        _currentTeam.RotateOrderList();
        return false;
    }

    private void ExecuteOffensiveSkill(IUnit currentUnit, object targetUnit, SkillData skillData)
    {
        ConsumeMp(currentUnit, skillData.cost);
        string affinity = _attackProcessor.ApplyOffensiveSkill(currentUnit, targetUnit, skillData, _currentTeam.UsedSkillsCount);
        _currentTeam.IncrementUsedSkillsCount();
        ConsumeSkillTurns(affinity);
    }

    private bool HandleRegularHealingSkill(object currentUnit, SkillData skillData, bool isHealSkill, bool isReviveSkill)
    {
        int targetIndex = _allySelectorController.ChooseAllyToHeal(currentUnit, !isHealSkill);
        if (targetIndex == ActionConstantsData.CancelTargetSelection)
        {
            _gameUi.PrintLine();
            ExecuteUnitTurn();
            return true;
        }

        _gameUi.PrintLine();
        object targetUnit = _allySelectorController.GetAlly(targetIndex);

        ExecuteHealingSkill(currentUnit, targetUnit, skillData, isHealSkill, isReviveSkill);
        _currentTeam.RotateOrderList();
        _currentTeam.IncrementUsedSkillsCount();
        return false;
    }

    private void ExecuteHealingSkill(object currentUnit, object targetUnit, SkillData skillData, bool isHealSkill, bool isReviveSkill)
    {
        ConsumeMp(currentUnit, skillData.cost);

        string healerName = _gameUi.GetUnitName(currentUnit);
        string targetName = _gameUi.GetUnitName(targetUnit);

        if (isReviveSkill)
        {
            ReviveTarget(targetUnit, skillData);
        }
        else if (isHealSkill)
        {
            _gameUi.WriteLine($"{healerName} cura a {targetName}");
            HealTarget(targetUnit, skillData);
        }

        ConsumeHealingSkillTurns();
    }

    private void ConsumeHealingSkillTurns()
    {
        if (_currentTeam.BlinkingTurns > 0)
            _currentTeam.ConsumeBlinkingTurn();
        else
            _currentTeam.ConsumeFullTurn();
    }

    private bool HandleInvitationSkillExecution(object currentUnit, SkillData skillData)
    {
        bool wasCancelled = WasInvitationSkillCancelled(currentUnit);
        if (wasCancelled)
        {
            _gameUi.PrintLine();
            ExecuteUnitTurn();
            return true;
        }
        
        ConsumeMp(currentUnit, skillData.cost);
        _currentTeam.RotateOrderList();
        _currentTeam.IncrementUsedSkillsCount();
        return false;
    }

    private void ExecuteInvitationSkill(object currentUnit, Monster selectedMonster, int positionSelection)
    {
        _gameUi.PrintLine();
        _gameUi.DisplaySummonSuccess(selectedMonster.Name);
        
        if (selectedMonster.IsDead())
        {
            ReviveMonsterWithInvitation(currentUnit, selectedMonster);
        }
        
        _currentTeam.PlaceMonsterInPosition(selectedMonster, positionSelection - 1);
        ConsumeSkillTurns("normal");
    }

    private void ReviveMonsterWithInvitation(object currentUnit, Monster selectedMonster)
    {
        string healerName = _gameUi.GetUnitName(currentUnit);
        _gameUi.WriteLine($"{healerName} revive a {selectedMonster.Name}");
        int healAmount = selectedMonster.OriginalHp;
        selectedMonster.Hp = healAmount;
        _gameUi.WriteLine($"{selectedMonster.Name} recibe {healAmount} de HP");
        _gameUi.WriteLine($"{selectedMonster.Name} termina con HP:{selectedMonster.Hp}/{selectedMonster.OriginalHp}");
    }

    private bool HandleSamuraiInvocation(Monster selectedMonster)
    {
        _gameUi.PrintLine();
        int selectedPosition = _gameUi.DisplayPositionMenu(_currentTeam);
        
        if (selectedPosition == ActionConstantsData.CancelInvokeSelection)
        {
            _gameUi.PrintLine();
            ExecuteUnitTurn();
            return true;
        }
        
        ExecuteSamuraiInvocation(selectedMonster, selectedPosition);
        return false;
    }

    private void ExecuteSamuraiInvocation(Monster selectedMonster, int selectedPosition)
    {
        _currentTeam.PlaceMonsterInPosition(selectedMonster, selectedPosition - 1);
        _gameUi.PrintLine();
        _gameUi.DisplaySummonSuccess(selectedMonster.Name);
        _currentTeam.ConsumeSummonTurns();
    }

    private void HandleMonsterInvocation(Monster currentMonster, Monster selectedMonster)
    {
        _currentTeam.SwapMonsters(currentMonster, selectedMonster);
        _gameUi.PrintLine();
        _gameUi.DisplaySummonSuccess(selectedMonster.Name);
        _currentTeam.ConsumeSummonTurns();
    }

    private bool HandleSabbatmaSkillExecution(IUnit currentUnit, SkillData skillData)
    {
        List<Monster> availableMonsters = _currentTeam.GetAvailableMonstersForSummon();
        int selectedMonsterIndex = _gameUi.DisplaySummonMenu(availableMonsters);
        
        if (selectedMonsterIndex == availableMonsters.Count + 1)
        {
            _gameUi.PrintLine();
            ExecuteUnitTurn();
            return true;
        }

        Monster selectedMonster = availableMonsters[selectedMonsterIndex - 1];
        _gameUi.PrintLine();
        
        int selectedPosition = _gameUi.DisplayPositionMenu(_currentTeam);
        if (selectedPosition == ActionConstantsData.CancelInvokeSelection)
        {
            _gameUi.PrintLine();
            ExecuteUnitTurn();
            return true;
        }
        
        ExecuteSabbatmaSkill(currentUnit, selectedMonster, selectedPosition, skillData);
        return false;
    }

    private void ExecuteSabbatmaSkill(IUnit currentUnit, Monster selectedMonster, int selectedPosition, SkillData skillData)
    {
        _currentTeam.PlaceMonsterInPosition(selectedMonster, selectedPosition - 1);
        _gameUi.PrintLine();
        _gameUi.DisplaySummonSuccess(selectedMonster.Name);
        _currentTeam.ConsumeNonOffensiveSkillsTurns();
        ConsumeMp(currentUnit, skillData.cost);
        _currentTeam.IncrementUsedSkillsCount();
    }
    private List<Monster> GetAllReserveMonsters()
    {
        List<Monster> reserveMonsters = new List<Monster>();
        int frontLineCount = Math.Min(_currentTeam.Units.Count, 3);
    
        // Añadir monstruos muertos del frente (posiciones 0-2)
        for (int i = 0; i < frontLineCount; i++)
        {
            var monster = _currentTeam.Units[i];
            if (monster.IsDead() && monster.Name != "Placeholder")
                reserveMonsters.Add(monster);
        }
    
        // Añadir todos los monstruos de la reserva (posiciones 3+)
        for (int i = frontLineCount; i < _currentTeam.Units.Count; i++)
        {
            var monster = _currentTeam.Units[i];
            if (monster.Name != "Placeholder")
                reserveMonsters.Add(monster);
        }

        // ✅ CORRECCIÓN: Ordenar por el orden original del archivo
        reserveMonsters = reserveMonsters.OrderBy(monster => 
        {
            int originalIndex = _currentTeam._originalMonstersOrder.IndexOf(monster.Name);
            return originalIndex == -1 ? _currentTeam._originalMonstersOrder.Count : originalIndex;
        }).ToList();

        return reserveMonsters;
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
    
    private void HealTarget(object target, SkillData skillData)
    {
        int maxHp = 0;
        if (target is Samurai samuraiTarget)
            maxHp = samuraiTarget.OriginalHp;
        else if (target is Monster monsterTarget)
            maxHp = monsterTarget.OriginalHp;

        int healAmount = (int)(maxHp * skillData.power / 100.0);

        string targetName = _gameUi.GetUnitName(target);
        _gameUi.WriteLine($"{targetName} recibe {healAmount} de HP");

        int originalHp = 0, currentHp = 0;

        if (target is Samurai samuraiTarget2)
        {
            originalHp = samuraiTarget2.OriginalHp;
            samuraiTarget2.Hp += healAmount;
            if (samuraiTarget2.Hp > originalHp) 
                samuraiTarget2.Hp = originalHp;
            currentHp = samuraiTarget2.Hp;
        }
        else if (target is Monster monsterTarget2)
        {
            originalHp = monsterTarget2.OriginalHp;
            monsterTarget2.Hp += healAmount;
            if (monsterTarget2.Hp > originalHp) 
                monsterTarget2.Hp = originalHp;
            currentHp = monsterTarget2.Hp;
        }

        _gameUi.ShowDamageResult(targetName, currentHp, originalHp);
    }

    private void ReviveTarget(object target, SkillData skill)
    {
        string targetName = _gameUi.GetUnitName(target);
        string reviverName = _gameUi.GetUnitName(_currentTeam.OrderList[0]);
        int maxHp = 0;
        int healAmount = 0;
        float revivePercentage = skill.power / 100.0f;

        if (target is Samurai samuraiTarget)
        {
            maxHp = samuraiTarget.OriginalHp;
            healAmount = (int)(maxHp * revivePercentage);
            samuraiTarget.Hp = healAmount;
            if (!_currentTeam.OrderList.Contains(samuraiTarget))
                _currentTeam.OrderList.Add(samuraiTarget);
        }
        else if (target is Monster monsterTarget)
        {
            maxHp = monsterTarget.OriginalHp;
            healAmount = (int)(maxHp * revivePercentage);
            monsterTarget.Hp = healAmount;
            int monsterIndex = _currentTeam.Units.IndexOf(monsterTarget);
            if (monsterIndex >= 0 && monsterIndex < 3)
            {
                Monster placeholderMonster = new Monster(monsterTarget.Name);
                placeholderMonster.Hp = 0;
                _currentTeam.Units[monsterIndex] = placeholderMonster;
                _currentTeam.Units.Add(monsterTarget);
            }
        }

        _gameUi.WriteLine($"{reviverName} revive a {targetName}");
        _gameUi.WriteLine($"{targetName} recibe {healAmount} de HP");
        _gameUi.ShowDamageResult(targetName, healAmount, maxHp);
    }
}