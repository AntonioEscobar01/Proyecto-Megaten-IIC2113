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
        // Esta funcionalidad aún no está implementada
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