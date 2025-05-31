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
        return actionCode != ActionConstantsData.PassTurnAction && 
               actionCode != ActionConstantsData.PassTurnActionMonster;
    }

    private TurnState CaptureInitialTurnState()
    {
        return new TurnState(_currentTeam.FullTurns, _currentTeam.BlinkingTurns);
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
        _gameUi.PrintTurnsUsed(_currentTeam, 
                              turnConsumption.FullTurnsUsed, 
                              turnConsumption.BlinkingTurnsUsed, 
                              turnConsumption.BlinkingTurnsGained);
    }

    private TurnConsumption CalculateTurnConsumption(TurnState initialState)
    {
        int fullTurnsUsed = _currentTeam.FullTurns - initialState.FullTurns;
        int blinkingTurnsUsed = CalculateBlinkingTurnsUsed(initialState.BlinkingTurns);
        int blinkingTurnsGained = CalculateBlinkingTurnsGained(initialState, blinkingTurnsUsed);
        
        return new TurnConsumption(fullTurnsUsed, blinkingTurnsUsed, blinkingTurnsGained);
    }

    private int CalculateBlinkingTurnsUsed(int initialBlinkingTurns)
    {
        int blinkingTurnsUsed = initialBlinkingTurns - _currentTeam.BlinkingTurns;
        return Math.Max(0, blinkingTurnsUsed);
    }

    private int CalculateBlinkingTurnsGained(TurnState initialState, int blinkingTurnsUsed)
    {
        int blinkingTurnsGained = (_currentTeam.BlinkingTurns - initialState.BlinkingTurns) + blinkingTurnsUsed;
        return Math.Max(0, blinkingTurnsGained);
    }

    private bool ProcessAttackAction(object currentUnit, int action)
    {
        int targetIndex = _targetSelectorController.ChooseUnitToAttack(currentUnit);

        if (targetIndex == ActionConstantsData.CancelTargetSelection)
        {
            _gameUi.PrintLine();
            ExecuteUnitTurn();
            return true;
        }

        _gameUi.PrintLine();
        object targetUnit = _targetSelectorController.GetTarget(targetIndex);

        string attackType = (action == ActionConstantsData.AttackAction) ? "Phys" : "Gun";
        string affinity = GetTargetAffinity(targetUnit, attackType);

        if (action == ActionConstantsData.AttackAction)
            _attackProcessor.Attack(currentUnit, targetUnit);
        else
            _attackProcessor.Shoot(currentUnit, targetUnit);

        ConsumeSkillTurns(affinity);
        _currentTeam.RotateOrderList();
        return false;
    }
    
    private string GetTargetAffinity(object target, string attackType)
    {
        if (target is Samurai samurai)
            return samurai.Affinities.GetAffinity(attackType);
        else if (target is Monster monster)
            return monster.Affinities.GetAffinity(attackType);
    
        return "-";
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

        string selectedSkillName = skillInfo.Abilities[skillInfo.SelectedIndex - 1];
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
        else if (skillData.type == "Heal")
        {
            return WasHealingSkillCancelled(currentUnit, skillData);
        }
        else if (skillData.type == "Special")
        {
            return WasSpecialSkillCancelled(currentUnit, skillData);
        }

        _currentTeam.RotateOrderList();
        return false;
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

    private bool IsTargetSelectionCancelled(int targetIndex)
    {
        return targetIndex == ActionConstantsData.CancelTargetSelection;
    }

    private void ExecuteOffensiveSkillOnTarget(IUnit currentUnit, int targetIndex, SkillData skillData)
    {
        object targetUnit = _targetSelectorController.GetTarget(targetIndex);
        ExecuteOffensiveSkill(currentUnit, targetUnit, skillData);
    }

    private void ExecuteOffensiveSkill(IUnit currentUnit, object targetUnit, SkillData skillData)
    {
        ConsumeMp(currentUnit, skillData.cost);
        string affinity = _attackProcessor.ApplyOffensiveSkill(currentUnit, targetUnit, skillData, _currentTeam.UsedSkillsCount);
        _currentTeam.IncrementUsedSkillsCount();
        ConsumeSkillTurns(affinity);
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
        int selectedMonsterIndex = SelectMonsterForInvocation(availableMonsters);
        
        if (IsMonsterSelectionCancelled(selectedMonsterIndex, availableMonsters.Count))
        {
            CancelAndReturnToMenu();
            return true;
        }

        Monster selectedMonster = availableMonsters[selectedMonsterIndex - 1];
        return ProcessInvocationByUnitType(currentUnit, selectedMonster, isSamurai);
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

    private bool ProcessInvocationByUnitType(IUnit currentUnit, Monster selectedMonster, bool isSamurai)
    {
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
        _gameUi.DisplaySummonSuccess(selectedMonster.Name);
    }

    private void HandleMonsterInvocation(Monster currentMonster, Monster selectedMonster)
    {
        _currentTeam.SwapMonsters(currentMonster, selectedMonster);
        ShowInvocationSuccess(selectedMonster);
        _currentTeam.ConsumeSummonTurns();
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
        _gameUi.ShowSurrenderMessage(_currentTeam.Samurai.Name, _currentTeam.Player);
        ShouldEndGame = true;
    }
    
    private bool WasHealingSkillCancelled(object currentUnit, SkillData skillData)
    {
        var skillClassification = ClassifyHealingSkill(skillData.name);
        
        if (skillClassification.IsInvitationSkill)
        {
            return HandleInvitationSkillExecution(currentUnit, skillData);
        }
        else
        {
            return HandleRegularHealingSkill(currentUnit, skillData, skillClassification);
        }
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

    private bool HandleRegularHealingSkill(object currentUnit, SkillData skillData, HealingSkillClassification classification)
    {
        int targetIndex = SelectHealingTarget(currentUnit, classification.IsHealSkill);
        if (IsTargetSelectionCancelled(targetIndex))
        {
            CancelAndReturnToMenu();
            return true;
        }

        ExecuteHealingSkillOnTarget(currentUnit, targetIndex, skillData, classification);
        CompleteHealingSkillExecution();
        return false;
    }

    private int SelectHealingTarget(object currentUnit, bool isHealSkill)
    {
        return _allySelectorController.ChooseAllyToHeal(currentUnit, !isHealSkill);
    }

    private void ExecuteHealingSkillOnTarget(object currentUnit, int targetIndex, SkillData skillData, HealingSkillClassification classification)
    {
        _gameUi.PrintLine();
        object targetUnit = _allySelectorController.GetAlly(targetIndex);
        ExecuteHealingSkill(currentUnit, targetUnit, skillData, classification);
    }

    private void ExecuteHealingSkill(object currentUnit, object targetUnit, SkillData skillData, HealingSkillClassification classification)
    {
        ConsumeMp(currentUnit, skillData.cost);
        string healerName = _gameUi.GetUnitName(currentUnit);
        string targetName = _gameUi.GetUnitName(targetUnit);

        if (classification.IsReviveSkill)
        {
            ReviveTarget(targetUnit, skillData);
        }
        else if (classification.IsHealSkill)
        {
            _gameUi.ShowHealingAction(healerName, targetName);
            HealTarget(targetUnit, skillData);
        }

        ConsumeHealingSkillTurns();
    }

    private void CompleteHealingSkillExecution()
    {
        _currentTeam.RotateOrderList();
        _currentTeam.IncrementUsedSkillsCount();
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
            CancelAndReturnToMenu();
            return true;
        }
        
        ConsumeMp(currentUnit, skillData.cost);
        CompleteHealingSkillExecution();
        return false;
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
        if (positionSelection == 4)
            return true;

        ExecuteInvitationSkill(currentUnit, selectedMonster, positionSelection);
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
        _gameUi.ShowReviveAction(healerName, selectedMonster.Name);
        int healAmount = selectedMonster.OriginalHp;
        selectedMonster.Hp = healAmount;
        _gameUi.ShowHealAmountReceived(selectedMonster.Name, healAmount);
        _gameUi.ShowHpResult(selectedMonster.Name, selectedMonster.Hp, selectedMonster.OriginalHp);
    }

    private bool WasSpecialSkillCancelled(IUnit currentUnit, SkillData skillData)
    {
        if (skillData.name == "Sabbatma")
        {
            return HandleSabbatmaSkillExecution(currentUnit, skillData);
        }

        return false;
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

    private bool ProcessSabbatmaPositionSelection(IUnit currentUnit, Monster selectedMonster, SkillData skillData)
    {
        _gameUi.PrintLine();
        int selectedPosition = SelectInvocationPosition();
        
        if (IsPositionSelectionCancelled(selectedPosition))
        {
            CancelAndReturnToMenu();
            return true;
        }
        
        ExecuteSabbatmaSkill(currentUnit, selectedMonster, selectedPosition, skillData);
        return false;
    }

    private void ExecuteSabbatmaSkill(IUnit currentUnit, Monster selectedMonster, int selectedPosition, SkillData skillData)
    {
        PlaceMonsterAndShowSuccess(selectedMonster, selectedPosition);
        ConsumeSabbatmaResources(currentUnit, skillData);
    }

    private void PlaceMonsterAndShowSuccess(Monster selectedMonster, int selectedPosition)
    {
        _currentTeam.PlaceMonsterInPosition(selectedMonster, selectedPosition - 1);
        ShowInvocationSuccess(selectedMonster);
    }

    private void ConsumeSabbatmaResources(IUnit currentUnit, SkillData skillData)
    {
        _currentTeam.ConsumeNonOffensiveSkillsTurns();
        ConsumeMp(currentUnit, skillData.cost);
        _currentTeam.IncrementUsedSkillsCount();
    }

    private List<Monster> GetAllReserveMonsters()
    {
        List<Monster> reserveMonsters = new List<Monster>();
        int frontLineCount = Math.Min(_currentTeam.Units.Count, 3);
    
        for (int i = 0; i < frontLineCount; i++)
        {
            var monster = _currentTeam.Units[i];
            if (monster.IsDead() && monster.Name != "Placeholder")
                reserveMonsters.Add(monster);
        }
    
        for (int i = frontLineCount; i < _currentTeam.Units.Count; i++)
        {
            var monster = _currentTeam.Units[i];
            if (monster.Name != "Placeholder")
                reserveMonsters.Add(monster);
        }

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
        _gameUi.ShowSkillSelectionPrompt(unitSkillInfo.Name);
    
        List<string> affordableAbilities = FilterAffordableAbilities(unitSkillInfo);
        DisplayAffordableSkills(affordableAbilities);
        _gameUi.ShowSkillCancelOption(affordableAbilities.Count + 1);

        int selectedOption = ReadSkillSelectionInput();
        return ProcessSkillSelectionInput(selectedOption, affordableAbilities, unitSkillInfo.Abilities);
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
        if (CanConsumeFullTurn())
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

    private void HandleNeutralOrResistAffinity()
    {
        ConsumeSingleTurnWithBlinkingPreference();
    }

    private bool CanConsumeFullTurn()
    {
        return _currentTeam.FullTurns < _currentTeam.MaxFullTurns;
    }

    private bool HasBlinkingTurns()
    {
        return _currentTeam.BlinkingTurns > 0;
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
        int blinkingTurnsToConsume = _currentTeam.BlinkingTurns;
        for (int i = 0; i < blinkingTurnsToConsume; i++)
        {
            _currentTeam.ConsumeBlinkingTurn();
        }
    }

    private void ConsumeAllRemainingFullTurns()
    {
        while (_currentTeam.FullTurns < _currentTeam.MaxFullTurns)
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
        return Math.Min(requestedCount, _currentTeam.BlinkingTurns);
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
        int fullTurnsAvailable = _currentTeam.MaxFullTurns - _currentTeam.FullTurns;
        int fullTurnsToConsume = Math.Min(remainingTurnsNeeded, fullTurnsAvailable);
        
        for (int i = 0; i < fullTurnsToConsume; i++)
        {
            _currentTeam.ConsumeFullTurn();
        }
    }
    
    private void HealTarget(object target, SkillData skillData)
    {
        int healAmount = CalculateHealAmount(target, skillData);
        _gameUi.ShowHealMessage(target, healAmount);
        ApplyHealingToTarget(target, healAmount);
        ShowHealingResult(target);
    }

    private int CalculateHealAmount(object target, SkillData skillData)
    {
        int maxHp = GetUnitMaxHp(target);
        return (int)(maxHp * skillData.power / 100.0);
    }

    private int GetUnitMaxHp(object unit)
    {
        return unit switch
        {
            Samurai samurai => samurai.OriginalHp,
            Monster monster => monster.OriginalHp,
            _ => 0
        };
    }

    

    private void ApplyHealingToTarget(object target, int healAmount)
    {
        switch (target)
        {
            case Samurai samurai:
                ApplyHealingToSamurai(samurai, healAmount);
                break;
            case Monster monster:
                ApplyHealingToMonster(monster, healAmount);
                break;
        }
    }

    private void ApplyHealingToSamurai(Samurai samurai, int healAmount)
    {
        samurai.Hp += healAmount;
        if (samurai.Hp > samurai.OriginalHp)
            samurai.Hp = samurai.OriginalHp;
    }

    private void ApplyHealingToMonster(Monster monster, int healAmount)
    {
        monster.Hp += healAmount;
        if (monster.Hp > monster.OriginalHp)
            monster.Hp = monster.OriginalHp;
    }

    private void ShowHealingResult(object target)
    {
        string targetName = _gameUi.GetUnitName(target);
        int currentHp = GetUnitCurrentHp(target);
        int originalHp = GetUnitMaxHp(target);
        _gameUi.ShowHpResult(targetName, currentHp, originalHp);
    }

    private int GetUnitCurrentHp(object unit)
    {
        return unit switch
        {
            Samurai samurai => samurai.Hp,
            Monster monster => monster.Hp,
            _ => 0
        };
    }

    private void ReviveTarget(object target, SkillData skill)
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

    private ReviveData PrepareReviveData(object target, SkillData skill)
    {
        string targetName = _gameUi.GetUnitName(target);
        string reviverName = _gameUi.GetUnitName(_currentTeam.OrderList[0]);
        int maxHp = GetUnitMaxHp(target);
        float revivePercentage = skill.power / 100.0f;
        int healAmount = (int)(maxHp * revivePercentage);
        
        return new ReviveData(targetName, reviverName, maxHp, healAmount);
    }

    private void ReviveSamurai(Samurai samurai, ReviveData reviveData)
    {
        samurai.Hp = reviveData.HealAmount;
        AddSamuraiToOrderListIfNeeded(samurai);
    }

    private void AddSamuraiToOrderListIfNeeded(Samurai samurai)
    {
        if (!_currentTeam.OrderList.Contains(samurai))
            _currentTeam.OrderList.Add(samurai);
    }

    private void ReviveMonster(Monster monster, ReviveData reviveData)
    {
        monster.Hp = reviveData.HealAmount;
        HandleMonsterRevivalPositioning(monster);
    }

    private void HandleMonsterRevivalPositioning(Monster monster)
    {
        int monsterIndex = _currentTeam.Units.IndexOf(monster);
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
        Monster placeholderMonster = CreateDeadPlaceholder(monster.Name);
        _currentTeam.Units[monsterIndex] = placeholderMonster;
    }

    private Monster CreateDeadPlaceholder(string monsterName)
    {
        Monster placeholder = new Monster(monsterName);
        placeholder.Hp = 0;
        return placeholder;
    }

    private void MoveMonsterToReserve(Monster monster)
    {
        _currentTeam.Units.Add(monster);
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
            int abilityCost = _skillsManager.GetSkillCost(ability);

            if (abilityCost <= skillInfo.CurrentMp)
            {
                affordableAbilities.Add(ability);
            }
        }
        
        return affordableAbilities;
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
