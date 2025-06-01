namespace Shin_Megami_Tensei;

public class Team
{
    private string _player;
    private Samurai? _samurai;
    private readonly TeamTurnManager _turnManager;
    private readonly BattleOrderManager _orderManager;
    private readonly MonsterPositionManager _positionManager;
    private readonly TeamValidatorManager _validatorManager;
    private readonly TeamBuilderManager _builderManager;

    public Team(string player)
    {
        _player = player;
        _turnManager = new TeamTurnManager();
        _orderManager = new BattleOrderManager();
        _positionManager = new MonsterPositionManager();
        _validatorManager = new TeamValidatorManager();
        _builderManager = new TeamBuilderManager();
    }
    
    public string GetPlayer() => _player;
    public Samurai? GetSamurai() => _samurai;
    public List<Monster> GetUnits() => _positionManager.GetUnits();
    public List<string> GetOriginalMonstersOrder() => _positionManager.GetOriginalMonstersOrder();
    
    public bool IsSamuraiAlive()
    {
        return _samurai != null && !_samurai.IsDead();
    }

    public bool IsSamuraiDead()
    {
        return _samurai != null && _samurai.IsDead();
    }

    public bool HasSamurai()
    {
        return _samurai != null;
    }

    public string GetSamuraiName()
    {
        return _samurai?.GetName() ?? "";
    }
    
    public bool IsMonsterAliveAtPosition(int position)
    {
        return _positionManager.IsMonsterAliveAtPosition(position);
    }

    public Monster GetMonsterAtPosition(int position)
    {
        return _positionManager.GetMonsterAtPosition(position);
    }

    public bool IsPositionValid(int position)
    {
        return _positionManager.IsPositionValid(position);
    }

    // Turn management delegation
    public int GetFullTurns() => _turnManager.GetFullTurns();
    public int GetBlinkingTurns() => _turnManager.GetBlinkingTurns();
    public int GetMaxFullTurns() => _turnManager.GetMaxFullTurns();
    public int GetUsedSkillsCount() => _turnManager.GetUsedSkillsCount();

    public void SetMaxFullTurns()
    {
        _turnManager.SetMaxFullTurns(_orderManager.GetOrderList().Count);
    }

    public void ResetTurns()
    {
        _turnManager.ResetTurns();
    }

    public bool HasCompletedAllTurns()
    {
        return _turnManager.HasCompletedAllTurns();
    }

    public void ConsumeFullTurn()
    {
        _turnManager.ConsumeFullTurn();
    }

    public void ConsumeBlinkingTurn()
    {
        _turnManager.ConsumeBlinkingTurn();
    }

    public void AddBlinkingTurn()
    {
        _turnManager.AddBlinkingTurn();
    }

    public void IncrementUsedSkillsCount()
    {
        _turnManager.IncrementUsedSkillsCount();
    }

    public void ConsumeSummonTurns()
    {
        _turnManager.ConsumeSummonTurns();
        _orderManager.RotateOrderList();
    }
    
    public void ConsumeNonOffensiveSkillsTurns()
    {
        _turnManager.ConsumeNonOffensiveSkillsTurns();
        _orderManager.RotateOrderList();
    }
    
    public List<IUnit> GetOrderList() => _orderManager.GetOrderList();
    public IUnit? GetCurrentUnit() => _orderManager.GetCurrentUnit();

    public void InitializeOrderList()
    {
        _orderManager.InitializeOrderList(_samurai, _positionManager.GetUnits());
    }

    public void RotateOrderList()
    {
        _orderManager.RotateOrderList();
    }

    public void RemoveDeadUnits()
    {
        _orderManager.RemoveDeadUnits();
    }

    public bool AreAllUnitsDead()
    {
        return _orderManager.AreAllUnitsDead();
    }
    
    public List<Monster> GetAvailableMonstersForSummon()
    {
        return _positionManager.GetAvailableMonstersForSummon();
    }

    public void PlaceMonsterInPosition(Monster summonedMonster, int position)
    {
        _positionManager.PlaceMonsterInPosition(summonedMonster, position, _orderManager);
    }

    public void SwapMonsters(Monster currentMonster, Monster summonedMonster)
    {
        _positionManager.SwapMonsters(currentMonster, summonedMonster, _orderManager);
    }


    public bool IsValidTeam(List<string> possibleUnits)
    {
        var monsterNames = _builderManager.ExtractMonsterNames(possibleUnits);
        _positionManager.SetOriginalMonstersOrder(monsterNames);
        
        if (!_validatorManager.IsValidTeam(possibleUnits))
            return false;
            
        BuildTeamFromValidatedUnits(possibleUnits);
        return true;
    }

    private void BuildTeamFromValidatedUnits(List<string> possibleUnits)
    {
        var (samurai, monsterNames) = _builderManager.BuildTeamFromUnits(possibleUnits);
        _samurai = samurai;
        
        foreach (var monsterName in monsterNames)
        {
            _positionManager.AddMonster(new Monster(monsterName));
        }
    }
}