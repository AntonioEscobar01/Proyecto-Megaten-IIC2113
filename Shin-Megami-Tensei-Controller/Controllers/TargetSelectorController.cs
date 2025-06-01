namespace Shin_Megami_Tensei;

public class TargetSelectorController
{
    private readonly GameUiFacade _gameUi;
    private readonly Team _enemyTeam;
    private List<IUnit> _availableTargets;

    public TargetSelectorController(GameUiFacade gameUi, Team enemyTeam)
    {
        _gameUi = gameUi;
        _enemyTeam = enemyTeam;
        _availableTargets = new List<IUnit>();
    }

    public int ChooseUnitToAttack(IUnit attacker)
    {
        string attackerName = attacker.GetName();
        _gameUi.ShowTargetSelectionPrompt(attackerName);

        _availableTargets = GetAvailableTargets();
        _gameUi.DisplayTargets(_availableTargets);
        return GetTargetSelectionResult();
    }

    private List<IUnit> GetAvailableTargets()
    {
        List<IUnit> targets = new List<IUnit>();
        
        if (_enemyTeam.IsSamuraiAlive())
        {
            targets.Add(_enemyTeam.GetSamurai());
        }

        AddAvailableMonsters(targets);
        return targets;
    }

    private void AddAvailableMonsters(List<IUnit> targets)
    {
        var units = _enemyTeam.GetUnits();
        int maxVisibleMonsters = Math.Min(units.Count, 3);
    
        for (int monsterIndex = 0; monsterIndex < maxVisibleMonsters; monsterIndex++)
        {
            var monster = units[monsterIndex];
            if (!monster.IsDead())
                targets.Add(monster);
        }
    }

    private int GetTargetSelectionResult()
    {
        int selection = int.Parse(_gameUi.ReadLine());

        if (selection == _availableTargets.Count + 1)
            return ActionConstantsData.CancelTargetSelection;
        
        if (IsValidTargetSelection(selection))
            return CalculateTargetIndex(selection);
    
        return ActionConstantsData.CancelTargetSelection;
    }
    
    private bool IsValidTargetSelection(int selection)
    {
        return selection > 0 && selection <= _availableTargets.Count;
    }

    private int CalculateTargetIndex(int selection)
    {
        var selectedTarget = _availableTargets[selection - 1];
        if (selectedTarget is Samurai)
            return 1;
        else
        {
            var units = _enemyTeam.GetUnits();
            return units.IndexOf((Monster)selectedTarget) + 2;
        }
    }

    public IUnit GetTarget(int targetIndex)
    {
        var units = _enemyTeam.GetUnits();
        return targetIndex switch
        {
            1 => _enemyTeam.GetSamurai(),
            var idx when idx > 1 && idx <= units.Count + 1 => units[targetIndex - 2],
            _ => null
        };
    }
}