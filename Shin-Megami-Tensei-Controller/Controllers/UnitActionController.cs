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
        var currentUnit = GetCurrentUnit();
        int selectedAction = GetUnitAction(currentUnit);
        ProcessAction(currentUnit, selectedAction);
    }

    private IUnit GetCurrentUnit()
    {
        var orderList = _currentTeam.GetOrderList();
        return orderList[0];
    }
    
    private int GetUnitAction(IUnit currentUnit)
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
        int actionCode = CalculateActionCode(currentUnit, action);
        PrintActionSeparatorIfNeeded(actionCode);
        var initialTurnState = CaptureInitialTurnState();

        bool actionCancelled = ExecuteActionByCode(currentUnit, action, actionCode);

        if (ShouldShowTurnSummary(actionCancelled))
        {
            ShowTurnConsumptionSummary(initialTurnState);
        }
    }

    private int CalculateActionCode(IUnit currentUnit, int action)
    {
        return currentUnit is Monster ? action + _actionConstantsData.MonsterOffset : action;
    }

    private void PrintActionSeparatorIfNeeded(int actionCode)
    {
        if (ShouldPrintSeparatorForAction(actionCode))
            _gameUi.PrintLine();
    }

    private bool ShouldPrintSeparatorForAction(int actionCode)
    {
        return !IsPassTurnAction(actionCode);
    }

    private bool IsPassTurnAction(int actionCode)
    {
        return actionCode == ActionConstantsData.PassTurnAction || 
               actionCode == ActionConstantsData.PassTurnActionMonster;
    }

    private TurnState CaptureInitialTurnState()
    {
        return new TurnState(_currentTeam.GetFullTurns(), _currentTeam.GetBlinkingTurns());
    }

    private bool ExecuteActionByCode(IUnit currentUnit, int action, int actionCode)
    {
        return actionCode switch
        {
            ActionConstantsData.AttackAction or 
            ActionConstantsData.ShootAction or 
            ActionConstantsData.AttackActionMonster 
                => ProcessAttackAction(currentUnit, action),
                
            ActionConstantsData.SkillAction or 
            ActionConstantsData.SkillActionMonster 
                => WasSkillActionCancelled(currentUnit),
                
            ActionConstantsData.InvokeAction or 
            ActionConstantsData.InvokeActionMonster 
                => WasInvokeActionCancelled(currentUnit),
                
            ActionConstantsData.PassTurnAction or 
            ActionConstantsData.PassTurnActionMonster 
                => ExecutePassTurnAction(),
                
            ActionConstantsData.SurrenderAction 
                => ExecuteSurrenderAction(),
                
            _ => false
        };
    }

    private bool ExecutePassTurnAction()
    {
        ProcessPassTurnAction();
        return false;
    }

    private bool ExecuteSurrenderAction()
    {
        ProcessSurrenderAction();
        return false;
    }

    private bool ShouldShowTurnSummary(bool actionCancelled)
    {
        return !actionCancelled && !ShouldEndGame;
    }

    private void ShowTurnConsumptionSummary(TurnState initialState)
    {
        var turnConsumption = CalculateTurnConsumption(initialState);
        var turnUsageInfo = new TurnUsageInfo(turnConsumption.FullTurnsUsed, 
            turnConsumption.BlinkingTurnsUsed, 
            turnConsumption.BlinkingTurnsGained);
        _gameUi.PrintTurnsUsed(turnUsageInfo);
    }

    private TurnConsumption CalculateTurnConsumption(TurnState initialState)
    {
        int fullTurnsUsed = _currentTeam.GetFullTurns() - initialState.FullTurns;
        int blinkingTurnsUsed = CalculateBlinkingTurnsUsed(initialState.BlinkingTurns);
        int blinkingTurnsGained = CalculateBlinkingTurnsGained(initialState, blinkingTurnsUsed);
        
        return new TurnConsumption(fullTurnsUsed, blinkingTurnsUsed, blinkingTurnsGained);
    }

    private int CalculateBlinkingTurnsUsed(int initialBlinkingTurns)
    {
        int blinkingTurnsUsed = initialBlinkingTurns - _currentTeam.GetBlinkingTurns();
        return Math.Max(0, blinkingTurnsUsed);
    }

    private int CalculateBlinkingTurnsGained(TurnState initialState, int blinkingTurnsUsed)
    {
        int blinkingTurnsGained = (_currentTeam.GetBlinkingTurns() - initialState.BlinkingTurns) + blinkingTurnsUsed;
        return Math.Max(0, blinkingTurnsGained);
    }

    private bool ProcessAttackAction(IUnit currentUnit, int action)
    {
        int targetIndex = _targetSelectorController.ChooseUnitToAttack(currentUnit);

        if (IsTargetSelectionCancelled(targetIndex))
        {
            _gameUi.PrintLine();
            ExecuteUnitTurn();
            return true;
        }

        _gameUi.PrintLine();
        IUnit targetUnit = _targetSelectorController.GetTarget(targetIndex);

        string attackType = GetAttackType(action);
        string affinity = targetUnit.GetAffinity(attackType);

        ExecuteAttackByType(currentUnit, targetUnit, action);
        ConsumeSkillTurns(affinity);
        _currentTeam.RotateOrderList();
        return false;
    }
    
    private bool IsTargetSelectionCancelled(int targetIndex)
    {
        return targetIndex == ActionConstantsData.CancelTargetSelection;
    }

    private string GetAttackType(int action)
    {
        return IsPhysicalAttack(action) ? "Phys" : "Gun";
    }
    
    private bool IsPhysicalAttack(int action)
    {
        return action == ActionConstantsData.AttackAction;
    }

    private void ExecuteAttackByType(IUnit currentUnit, IUnit targetUnit, int action)
    {
        if (IsPhysicalAttack(action))
            _attackProcessor.Attack(currentUnit, targetUnit);
        else
            _attackProcessor.Shoot(currentUnit, targetUnit);
    }

    private bool WasSkillActionCancelled(IUnit currentUnit)
    {
        var skillInfo = GetSkillSelection(currentUnit);
        _gameUi.PrintLine();
        
        if (IsSkillSelectionCancelled(skillInfo))
        {
            ExecuteUnitTurn();
            return true;
        }

        var abilities = skillInfo.Abilities;
        string selectedSkillName = abilities[skillInfo.SelectedIndex - 1];
        var skillData = _skillsManager.GetSkillByName(selectedSkillName);

        return ExecuteSelectedSkill(currentUnit, skillData);
    }
    
    private bool IsSkillSelectionCancelled(SkillSelectionResult skillInfo)
    {
        return skillInfo.SelectedIndex > skillInfo.Abilities.Count;
    }

    private bool ExecuteSelectedSkill(IUnit currentUnit, SkillData skillData)
    {
        if (IsOffensiveSkill(skillData.type))
        {
            return HandleOffensiveSkillExecution(currentUnit, skillData);
        }
        else if (IsHealingSkill(skillData.type))
        {
            return WasHealingSkillCancelled(currentUnit, skillData);
        }
        else if (IsSpecialSkill(skillData.type))
        {
            return WasSpecialSkillCancelled(currentUnit, skillData);
        }

        _currentTeam.RotateOrderList();
        return false;
    }
    
    private bool IsHealingSkill(string skillType)
    {
        return skillType == "Heal";
    }
    
    private bool IsSpecialSkill(string skillType)
    {
        return skillType == "Special";
    }

    private bool HandleOffensiveSkillExecution(IUnit currentUnit, SkillData skillData)
    {
        int targetIndex = SelectOffensiveSkillTarget(currentUnit);
        
        if (IsTargetSelectionCancelled(targetIndex))
        {
            ExecuteUnitTurn();
            return true;
        }

        ExecuteOffensiveSkillOnTarget(currentUnit, targetIndex, skillData);
        _currentTeam.RotateOrderList();
        return false;
    }

    private int SelectOffensiveSkillTarget(IUnit currentUnit)
    {
        int targetIndex = _targetSelectorController.ChooseUnitToAttack(currentUnit);
        _gameUi.PrintLine();
        return targetIndex;
    }

    private void ExecuteOffensiveSkillOnTarget(IUnit currentUnit, int targetIndex, SkillData skillData)
    {
        IUnit targetUnit = _targetSelectorController.GetTarget(targetIndex);
        ExecuteOffensiveSkill(currentUnit, targetUnit, skillData);
    }

    private void ExecuteOffensiveSkill(IUnit currentUnit, IUnit targetUnit, SkillData skillData)
    {
        currentUnit.ConsumeMp(skillData.cost);
        var skillInfo = new OffensiveSkillInfo(skillData, _currentTeam.GetUsedSkillsCount());
        string affinity = _attackProcessor.ApplyOffensiveSkill(currentUnit, targetUnit, skillInfo);
        _currentTeam.IncrementUsedSkillsCount();
        ConsumeSkillTurns(affinity);
    }

    private bool IsOffensiveSkill(string skillType)
    {
        return skillType == "Phys" || skillType == "Gun" || 
               skillType == "Fire" || skillType == "Ice" || 
               skillType == "Elec" || skillType == "Force";
    }
    
    private bool WasInvokeActionCancelled(IUnit currentUnit)
    {
        bool isSamurai = currentUnit is Samurai;
        List<Monster> availableMonsters = _currentTeam.GetAvailableMonstersForSummon();
        int selectedMonsterIndex = SelectMonsterForInvocation(availableMonsters);
    
        if (IsMonsterSelectionCancelled(selectedMonsterIndex, availableMonsters.Count))
        {
            CancelAndReturnToMenu();
            return true;
        }

        Monster selectedMonster = availableMonsters[selectedMonsterIndex - 1];
    
        if (isSamurai)
        {
            return ProcessSamuraiInvocation(currentUnit, selectedMonster);
        }
        else
        {
            return ProcessMonsterInvocation(currentUnit, selectedMonster);
        }
    }

    private int SelectMonsterForInvocation(List<Monster> availableMonsters)
    {
        return _gameUi.DisplaySummonMenu(availableMonsters);
    }
    
    private bool IsMonsterSelectionCancelled(int selectedIndex, int availableCount)
    {
        return selectedIndex == availableCount + 1;
    }

    private void CancelAndReturnToMenu()
    {
        _gameUi.PrintLine();
        ExecuteUnitTurn();
    }

    private bool ProcessSamuraiInvocation(IUnit currentUnit, Monster selectedMonster)
    {
        return HandleSamuraiInvocation(selectedMonster);
    }

    private bool ProcessMonsterInvocation(IUnit currentUnit, Monster selectedMonster)
    {
        HandleMonsterInvocation((Monster)currentUnit, selectedMonster);
        return false;
    }

    private bool HandleSamuraiInvocation(Monster selectedMonster)
    {
        _gameUi.PrintLine();
        int selectedPosition = SelectInvocationPosition();
        
        if (IsPositionSelectionCancelled(selectedPosition))
        {
            CancelAndReturnToMenu();
            return true;
        }
        
        ExecuteSamuraiInvocation(selectedMonster, selectedPosition);
        return false;
    }

    private int SelectInvocationPosition()
    {
        return _gameUi.DisplayPositionMenu(_currentTeam);
    }
    
    private bool IsPositionSelectionCancelled(int selectedPosition)
    {
        return selectedPosition == ActionConstantsData.CancelInvokeSelection;
    }

    private void ExecuteSamuraiInvocation(Monster selectedMonster, int selectedPosition)
    {
        _currentTeam.PlaceMonsterInPosition(selectedMonster, selectedPosition - 1);
        ShowInvocationSuccess(selectedMonster);
        _currentTeam.ConsumeSummonTurns();
    }

    private void ShowInvocationSuccess(Monster selectedMonster)
    {
        _gameUi.PrintLine();
        _gameUi.DisplaySummonSuccess(selectedMonster.GetName());
    }

    private void HandleMonsterInvocation(Monster currentMonster, Monster selectedMonster)
    {
        _currentTeam.SwapMonsters(currentMonster, selectedMonster);
        ShowInvocationSuccess(selectedMonster);
        _currentTeam.ConsumeSummonTurns();
    }

    private void ProcessPassTurnAction()
    {
        if (HasBlinkingTurns())
            _currentTeam.ConsumeBlinkingTurn();
        else
        {
            _currentTeam.ConsumeFullTurn();
            _currentTeam.AddBlinkingTurn();
        }

        _currentTeam.RotateOrderList();
    }
    
    private bool HasBlinkingTurns()
    {
        return _currentTeam.GetBlinkingTurns() > 0;
    }

    private void ProcessSurrenderAction()
    {
        _gameUi.ShowSurrenderMessage(_currentTeam.GetSamurai().GetName(), _currentTeam.GetPlayer());
        ShouldEndGame = true;
    }
    
    private bool WasHealingSkillCancelled(IUnit currentUnit, SkillData skillData)
    {
        var skillClassification = ClassifyHealingSkill(skillData.name);
        
        if (IsInvitationSkill(skillClassification))
        {
            return WasInvitationSkillExecutionCancelled(currentUnit, skillData);
        }
        else
        {
            return WasRegularHealingSkillCancelled(currentUnit, skillData, skillClassification);
        }
    }
    
    private bool IsInvitationSkill(HealingSkillClassification skillClassification)
    {
        return skillClassification.IsInvitationSkill;
    }

    private HealingSkillClassification ClassifyHealingSkill(string skillName)
    {
        string[] percentageHealSkills = { "Dia", "Diarama", "Diarahan" };
        string[] reviveHealSkills = { "Recarm", "Samarecarm" };
        
        return new HealingSkillClassification
        {
            IsHealSkill = percentageHealSkills.Contains(skillName),
            IsReviveSkill = reviveHealSkills.Contains(skillName),
            IsInvitationSkill = skillName == "Invitation"
        };
    }

    private bool WasRegularHealingSkillCancelled(IUnit currentUnit, SkillData skillData, HealingSkillClassification classification)
    {
        int targetIndex = GetHealingTargetIndex(currentUnit, classification);
        
        if (IsTargetSelectionCancelled(targetIndex))
        {
            CancelAndReturnToMenu();
            return true;
        }

        var healingInfo = new HealingSkillInfo(skillData, classification);
        ExecuteHealingSkillOnTarget(currentUnit, targetIndex, healingInfo);
        CompleteHealingSkillExecution();
        return false;
    }

    private int GetHealingTargetIndex(IUnit currentUnit, HealingSkillClassification classification)
    {
        return IsRegularHealSkill(classification)
            ? SelectHealingTargetForHealing(currentUnit)
            : SelectHealingTargetForReviving(currentUnit);
    }
    
    private bool IsRegularHealSkill(HealingSkillClassification classification)
    {
        return classification.IsHealSkill;
    }

    private int SelectHealingTargetForHealing(IUnit currentUnit)
    {
        return _allySelectorController.ChooseAllyToHeal(currentUnit);
    }

    private int SelectHealingTargetForReviving(IUnit currentUnit)
    {
        return _allySelectorController.ChooseAllyToRevive(currentUnit);
    }

    private void ExecuteHealingSkillOnTarget(IUnit currentUnit, int targetIndex, HealingSkillInfo healingInfo)
    {
        _gameUi.PrintLine();
        IUnit targetUnit = _allySelectorController.GetAlly(targetIndex);
        ExecuteHealingSkill(currentUnit, targetUnit, healingInfo);
    }

    private void ExecuteHealingSkill(IUnit currentUnit, IUnit targetUnit, HealingSkillInfo healingInfo)
    {
        currentUnit.ConsumeMp(healingInfo.SkillData.cost);
        string healerName = currentUnit.GetName();
        string targetName = targetUnit.GetName();

        if (IsReviveSkill(healingInfo.Classification))
        {
            ReviveTarget(targetUnit, healingInfo.SkillData);
        }
        else if (IsRegularHealSkill(healingInfo.Classification))
        {
            _gameUi.ShowHealingAction(healerName, targetName);
            HealTarget(targetUnit, healingInfo.SkillData);
        }

        ConsumeHealingSkillTurns();
    }
    
    private bool IsReviveSkill(HealingSkillClassification classification)
    {
        return classification.IsReviveSkill;
    }

    private void CompleteHealingSkillExecution()
    {
        _currentTeam.RotateOrderList();
        _currentTeam.IncrementUsedSkillsCount();
    }

    private void ConsumeHealingSkillTurns()
    {
        if (HasBlinkingTurns())
            _currentTeam.ConsumeBlinkingTurn();
        else
            _currentTeam.ConsumeFullTurn();
    }

    private bool WasInvitationSkillExecutionCancelled(IUnit currentUnit, SkillData skillData)
    {
        bool wasCancelled = WasInvitationSkillCancelled(currentUnit);
        if (wasCancelled)
        {
            CancelAndReturnToMenu();
            return true;
        }
        
        currentUnit.ConsumeMp(skillData.cost);
        CompleteHealingSkillExecution();
        return false;
    }

    private bool WasInvitationSkillCancelled(IUnit currentUnit)
    {
        List<Monster> availableMonsters = GetAllReserveMonsters();
        int monsterSelection = _gameUi.DisplaySummonMenu(availableMonsters);
        
        if (IsMonsterSelectionCancelled(monsterSelection, availableMonsters.Count))
        {
            return true;
        }
        
        Monster selectedMonster = availableMonsters[monsterSelection - 1];
        _gameUi.PrintLine();
        
        int positionSelection = _gameUi.DisplayPositionMenu(_currentTeam);
        if (IsInvitationPositionCancelled(positionSelection))
            return true;

        ExecuteInvitationSkill(currentUnit, selectedMonster, positionSelection);
        return false;
    }
    
    private bool IsInvitationPositionCancelled(int positionSelection)
    {
        return positionSelection == 4;
    }

    private void ExecuteInvitationSkill(IUnit currentUnit, Monster selectedMonster, int positionSelection)
    {
        _gameUi.PrintLine();
        _gameUi.DisplaySummonSuccess(selectedMonster.GetName());
        
        if (selectedMonster.IsDead())
        {
            ReviveMonsterWithInvitation(currentUnit, selectedMonster);
        }
        
        _currentTeam.PlaceMonsterInPosition(selectedMonster, positionSelection - 1);
        ConsumeSkillTurns("normal");
    }

    private void ReviveMonsterWithInvitation(IUnit currentUnit, Monster selectedMonster)
    {
        string healerName = currentUnit.GetName();
        _gameUi.ShowReviveAction(healerName, selectedMonster.GetName());
        int healAmount = selectedMonster.GetMaxHp();
        selectedMonster.Heal(healAmount);
        _gameUi.ShowHealAmountReceived(selectedMonster.GetName(), healAmount);
        _gameUi.ShowHpResult(selectedMonster.GetName(), selectedMonster.GetCurrentHp(), selectedMonster.GetMaxHp());
    }

    private bool WasSpecialSkillCancelled(IUnit currentUnit, SkillData skillData)
    {
        if (IsSabbatmaSkill(skillData.name))
        {
            return HandleSabbatmaSkillExecution(currentUnit, skillData);
        }

        return false;
    }
    
    private bool IsSabbatmaSkill(string skillName)
    {
        return skillName == "Sabbatma";
    }

    private bool HandleSabbatmaSkillExecution(IUnit currentUnit, SkillData skillData)
    {
        List<Monster> availableMonsters = _currentTeam.GetAvailableMonstersForSummon();
        int selectedMonsterIndex = SelectMonsterForSabbatma(availableMonsters);
        
        if (IsMonsterSelectionCancelled(selectedMonsterIndex, availableMonsters.Count))
        {
            CancelAndReturnToMenu();
            return true;
        }

        Monster selectedMonster = availableMonsters[selectedMonsterIndex - 1];
        return ProcessSabbatmaPositionSelection(currentUnit, selectedMonster, skillData);
    }

    private int SelectMonsterForSabbatma(List<Monster> availableMonsters)
    {
        return _gameUi.DisplaySummonMenu(availableMonsters);
    }

    private void ExecuteSabbatmaSkill(IUnit currentUnit, MonsterPlacement placement, SkillData skillData)
    {
        PlaceMonsterAndShowSuccess(placement.SelectedMonster, placement.SelectedPosition);
        ConsumeSabbatmaResources(currentUnit, skillData);
    }

    private void PlaceMonsterAndShowSuccess(Monster selectedMonster, int selectedPosition)
    {
        _currentTeam.PlaceMonsterInPosition(selectedMonster, selectedPosition - 1);
        ShowInvocationSuccess(selectedMonster);
    }

    private bool ProcessSabbatmaPositionSelection(IUnit currentUnit, Monster selectedMonster, SkillData skillData)
    {
        _gameUi.PrintLine();
        int selectedPosition = SelectInvocationPosition();
    
        if (IsPositionSelectionCancelled(selectedPosition))
        {
            CancelAndReturnToMenu();
            return true;
        }
    
        var placement = new MonsterPlacement(selectedMonster, selectedPosition);
        ExecuteSabbatmaSkill(currentUnit, placement, skillData);
        return false;
    }

    private void ConsumeSabbatmaResources(IUnit currentUnit, SkillData skillData)
    {
        _currentTeam.ConsumeNonOffensiveSkillsTurns();
        currentUnit.ConsumeMp(skillData.cost);
        _currentTeam.IncrementUsedSkillsCount();
    }

    private List<Monster> GetAllReserveMonsters()
    {
        List<Monster> reserveMonsters = new List<Monster>();
        var units = _currentTeam.GetUnits();
        int frontLineCount = Math.Min(units.Count, 3);

        AddDeadFrontlineMonsters(reserveMonsters, frontLineCount);
        AddReserveMonsters(reserveMonsters, frontLineCount);
    
        return SortReserveMonstersByOriginalOrder(reserveMonsters);
    }

    private void AddDeadFrontlineMonsters(List<Monster> reserveMonsters, int frontLineCount)
    {
        var units = _currentTeam.GetUnits();
        for (int i = 0; i < frontLineCount; i++)
        {
            var monster = units[i];
            if (IsDeadFrontlineMonster(monster))
                reserveMonsters.Add(monster);
        }
    }
    
    private bool IsDeadFrontlineMonster(Monster monster)
    {
        return monster.IsDead() && monster.GetName() != "Placeholder";
    }

    private void AddReserveMonsters(List<Monster> reserveMonsters, int frontLineCount)
    {
        var units = _currentTeam.GetUnits();
        for (int i = frontLineCount; i < units.Count; i++)
        {
            var monster = units[i];
            if (IsValidReserveMonster(monster))
                reserveMonsters.Add(monster);
        }
    }
    
    private bool IsValidReserveMonster(Monster monster)
    {
        return monster.GetName() != "Placeholder";
    }

    private List<Monster> SortReserveMonstersByOriginalOrder(List<Monster> reserveMonsters)
    {
        var sortedMonsters = new List<Monster>(reserveMonsters);
        var originalOrder = _currentTeam.GetOriginalMonstersOrder();
        
        sortedMonsters.Sort((monster1, monster2) => 
        {
            int index1 = GetMonsterOriginalIndex(monster1, originalOrder);
            int index2 = GetMonsterOriginalIndex(monster2, originalOrder);
            return index1.CompareTo(index2);
        });
        
        return sortedMonsters;
    }

    private int GetMonsterOriginalIndex(Monster monster, List<string> originalOrder)
    {
        int originalIndex = originalOrder.IndexOf(monster.GetName());
        return originalIndex == -1 ? originalOrder.Count : originalIndex;
    }
    
    private SkillSelectionResult GetSkillSelection(IUnit attacker)
    {
        var unitSkillInfo = CreateUnitSkillInfo(attacker);
        _gameUi.ShowSkillSelectionPrompt(unitSkillInfo.Name);
    
        List<string> affordableAbilities = FilterAffordableAbilities(unitSkillInfo);
        DisplayAffordableSkills(affordableAbilities);
        _gameUi.ShowSkillCancelOption(affordableAbilities.Count + 1);

        int selectedOption = ReadSkillSelectionInput();
        return ProcessSkillSelectionInput(selectedOption, affordableAbilities, unitSkillInfo.Abilities);
    }

    private UnitSkillInfo CreateUnitSkillInfo(IUnit attacker)
    {
        return new UnitSkillInfo(attacker.GetName(), attacker.GetAbilities(), attacker.GetCurrentMp());
    }
    
    private void ConsumeSkillTurns(string affinity)
    {
        switch (affinity)
        {
            case "Rp":
            case "Dr":
                HandleRepelOrDrainAffinity();
                break;

            case "Nu":
                HandleNullAffinity();
                break;

            case "Miss":
                HandleMissAffinity();
                break;

            case "Wk":
                HandleWeakAffinity();
                break;

            default:
                HandleNeutralOrResistAffinity();
                break;
        }
    }

    private void HandleRepelOrDrainAffinity()
    {
        ConsumeAllTurns();
    }

    private void HandleNullAffinity()
    {
        ConsumeMultipleBlinkingTurns(2);
    }

    private void HandleMissAffinity()
    {
        ConsumeSingleTurnWithBlinkingPreference();
    }

    private void HandleWeakAffinity()
    {
        if (CanConsumeMoreFullTurns())
        {
            ConsumeFullTurnAndGrantBlinkingTurn();
        }
        else if (HasBlinkingTurns())
        {
            _currentTeam.ConsumeBlinkingTurn();
        }
        else
        {
            _currentTeam.ConsumeFullTurn();
        }
    }
    
    private bool CanConsumeMoreFullTurns()
    {
        return _currentTeam.GetFullTurns() < _currentTeam.GetMaxFullTurns();
    }

    private void HandleNeutralOrResistAffinity()
    {
        ConsumeSingleTurnWithBlinkingPreference();
    }

    private void ConsumeFullTurnAndGrantBlinkingTurn()
    {
        _currentTeam.ConsumeFullTurn();
        _currentTeam.AddBlinkingTurn();
    }

    private void ConsumeSingleTurnWithBlinkingPreference()
    {
        if (HasBlinkingTurns())
            _currentTeam.ConsumeBlinkingTurn();
        else
            _currentTeam.ConsumeFullTurn();
    }

    private void ConsumeAllTurns()
    {
        ConsumeAllBlinkingTurns();
        ConsumeAllRemainingFullTurns();
    }

    private void ConsumeAllBlinkingTurns()
    {
        int blinkingTurnsToConsume = _currentTeam.GetBlinkingTurns();
        for (int i = 0; i < blinkingTurnsToConsume; i++)
        {
            _currentTeam.ConsumeBlinkingTurn();
        }
    }

    private void ConsumeAllRemainingFullTurns()
    {
        while (CanConsumeMoreFullTurns())
        {
            _currentTeam.ConsumeFullTurn();
        }
    }

    private void ConsumeMultipleBlinkingTurns(int count)
    {
        int blinkingTurnsToConsume = CalculateBlinkingTurnsToConsume(count);
        ConsumeBlinkingTurns(blinkingTurnsToConsume);
        
        int remainingTurnsNeeded = count - blinkingTurnsToConsume;
        if (remainingTurnsNeeded > 0)
        {
            ConsumeRemainingTurnsAsFullTurns(remainingTurnsNeeded);
        }
    }

    private int CalculateBlinkingTurnsToConsume(int requestedCount)
    {
        return Math.Min(requestedCount, _currentTeam.GetBlinkingTurns());
    }

    private void ConsumeBlinkingTurns(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _currentTeam.ConsumeBlinkingTurn();
        }
    }

    private void ConsumeRemainingTurnsAsFullTurns(int remainingTurnsNeeded)
    {
        int fullTurnsAvailable = _currentTeam.GetMaxFullTurns() - _currentTeam.GetFullTurns();
        int fullTurnsToConsume = Math.Min(remainingTurnsNeeded, fullTurnsAvailable);
        
        for (int i = 0; i < fullTurnsToConsume; i++)
        {
            _currentTeam.ConsumeFullTurn();
        }
    }
    
    private void HealTarget(IUnit target, SkillData skillData)
    {
        int healAmount = CalculateHealAmount(target, skillData);
        _gameUi.ShowHealMessage(target, healAmount);
        target.Heal(healAmount);
        ShowHealingResult(target);
    }

    private int CalculateHealAmount(IUnit target, SkillData skillData)
    {
        int maxHp = target.GetMaxHp();
        return (int)(maxHp * skillData.power / 100.0);
    }

    private void ShowHealingResult(IUnit target)
    {
        string targetName = target.GetName();
        int currentHp = target.GetCurrentHp();
        int originalHp = target.GetMaxHp();
        _gameUi.ShowHpResult(targetName, currentHp, originalHp);
    }

    private void ReviveTarget(IUnit target, SkillData skill)
    {
        var reviveData = PrepareReviveData(target, skill);
        
        if (target is Samurai samurai)
        {
            ReviveSamurai(samurai, reviveData);
        }
        else if (target is Monster monster)
        {
            ReviveMonster(monster, reviveData);
        }
        
        ShowReviveMessages(reviveData);
    }

    private ReviveData PrepareReviveData(IUnit target, SkillData skill)
    {
        string targetName = target.GetName();
        string reviverName = GetCurrentUnit().GetName();
        int maxHp = target.GetMaxHp();
        float revivePercentage = skill.power / 100.0f;
        int healAmount = (int)(maxHp * revivePercentage);
        
        return new ReviveData(targetName, reviverName, maxHp, healAmount);
    }

    private void ReviveSamurai(Samurai samurai, ReviveData reviveData)
    {
        int currentHp = samurai.GetCurrentHp();
        int damageToHeal = reviveData.HealAmount - currentHp;
        samurai.Heal(damageToHeal);
        AddSamuraiToOrderListIfNeeded(samurai);
    }

    private void AddSamuraiToOrderListIfNeeded(Samurai samurai)
    {
        var orderList = _currentTeam.GetOrderList();
        if (!orderList.Contains(samurai))
            orderList.Add(samurai);
    }

    private void ReviveMonster(Monster monster, ReviveData reviveData)
    {
        int currentHp = monster.GetCurrentHp();
        if (currentHp <= 0)
        {
            monster.Heal(reviveData.HealAmount);
        }
        else
        {
            int targetHp = reviveData.HealAmount;
            if (currentHp < targetHp)
            {
                monster.Heal(targetHp - currentHp);
            }
            else if (currentHp > targetHp)
            {
                monster.TakeDamage(currentHp - targetHp);
            }
        }
        HandleMonsterRevivalPositioning(monster);
    }

    private void HandleMonsterRevivalPositioning(Monster monster)
    {
        var units = _currentTeam.GetUnits();
        int monsterIndex = units.IndexOf(monster);
        if (IsMonsterInFrontline(monsterIndex))
        {
            ReplaceMonsterWithPlaceholder(monster, monsterIndex);
            MoveMonsterToReserve(monster);
        }
    }
    
    private bool IsMonsterInFrontline(int monsterIndex)
    {
        return monsterIndex >= 0 && monsterIndex < 3;
    }

    private void ReplaceMonsterWithPlaceholder(Monster monster, int monsterIndex)
    {
        Monster placeholderMonster = CreateDeadPlaceholder(monster.GetName());
        var units = _currentTeam.GetUnits();
        units[monsterIndex] = placeholderMonster;
    }

    private Monster CreateDeadPlaceholder(string monsterName)
    {
        Monster placeholder = new Monster("Placeholder");
        placeholder.TakeDamage(placeholder.GetCurrentHp());
        return placeholder;
    }

    private void MoveMonsterToReserve(Monster monster)
    {
        var units = _currentTeam.GetUnits();
        units.Add(monster);
    }

    private void AddMonsterToOrderListIfNeeded(Monster monster)
    {
        var orderList = _currentTeam.GetOrderList();
        if (!orderList.Contains(monster))
            orderList.Add(monster);
    }

    private void ShowReviveMessages(ReviveData reviveData)
    {
        _gameUi.ShowReviveAction(reviveData.ReviverName, reviveData.TargetName);
        _gameUi.ShowHealAmountReceived(reviveData.TargetName, reviveData.HealAmount);
        _gameUi.ShowHpResult(reviveData.TargetName, reviveData.HealAmount, reviveData.MaxHp);
    }

    private List<string> FilterAffordableAbilities(UnitSkillInfo skillInfo)
    {
        List<string> affordableAbilities = new List<string>();
    
        for (int abilityIndex = 0; abilityIndex < skillInfo.Abilities.Count; abilityIndex++)
        {
            string ability = skillInfo.Abilities[abilityIndex];
            if (CanAffordAbility(ability, skillInfo.CurrentMp))
                affordableAbilities.Add(ability);
        }
        return affordableAbilities;
    }
    
    private bool CanAffordAbility(string ability, int currentMp)
    {
        int abilityCost = _skillsManager.GetSkillCost(ability);
        return abilityCost <= currentMp;
    }

    private void DisplayAffordableSkills(List<string> affordableAbilities)
    {
        for (int i = 0; i < affordableAbilities.Count; i++)
        {
            string ability = affordableAbilities[i];
            int abilityCost = _skillsManager.GetSkillCost(ability);
            _gameUi.ShowAffordableSkill(i + 1, ability, abilityCost);
        }
    }

    private int ReadSkillSelectionInput()
    {
        return int.Parse(_gameUi.ReadLine());
    }

    private SkillSelectionResult ProcessSkillSelectionInput(int selectedOption, List<string> affordableAbilities, List<string> allAbilities)
    {
        if (IsSelectionCancelled(selectedOption, affordableAbilities.Count))
            return CreateCancelledSelection(allAbilities);

        if (IsValidSelection(selectedOption, affordableAbilities.Count))
        {
            return CreateValidSelection(selectedOption, affordableAbilities, allAbilities);
        }
        
        return CreateCancelledSelection(allAbilities);
    }
    
    private bool IsSelectionCancelled(int selectedOption, int affordableAbilitiesCount)
    {
        return selectedOption > affordableAbilitiesCount;
    }
    
    private bool IsValidSelection(int selectedOption, int affordableAbilitiesCount)
    {
        return selectedOption > 0 && selectedOption <= affordableAbilitiesCount;
    }

    private SkillSelectionResult CreateCancelledSelection(List<string> allAbilities)
    {
        return new SkillSelectionResult(allAbilities.Count + 1, allAbilities);
    }

    private SkillSelectionResult CreateValidSelection(int selectedOption, List<string> affordableAbilities, List<string> allAbilities)
    {
        string selectedAbility = affordableAbilities[selectedOption - 1];
        int originalIndex = allAbilities.IndexOf(selectedAbility);
        return new SkillSelectionResult(originalIndex + 1, allAbilities);
    }
}