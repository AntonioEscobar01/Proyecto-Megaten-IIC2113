namespace Shin_Megami_Tensei;

public class TargetSelectorController
{
    private readonly GameUi _gameUi;
    private readonly Team _enemyTeam;
    private List<IUnit> _availableTargets;

    public TargetSelectorController(GameUi gameUi, Team enemyTeam)
    {
        _gameUi = gameUi;
        _enemyTeam = enemyTeam;
        _availableTargets = new List<IUnit>();
    }

    public int ChooseUnitToAttack(IUnit attacker)
    {
        string attackerName = attacker.GetName();
        _gameUi.WriteLine($"Seleccione un objetivo para {attackerName}");

        _availableTargets = GetAvailableTargets();
        DisplayTargets();

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

    private void DisplayTargets()
    {
        for (int targetIndex = 0; targetIndex < _availableTargets.Count; targetIndex++)
        {
            DisplayTarget(_availableTargets[targetIndex], targetIndex);
        }
        _gameUi.WriteLine($"{_availableTargets.Count + 1}-Cancelar");
    }

    private void DisplayTarget(IUnit target, int targetIndex)
    {
        if (target is Samurai samurai)
        {
            _gameUi.WriteLine($"{targetIndex+1}-{samurai.GetName()} HP:{samurai.GetCurrentHp()}/{samurai.GetMaxHp()} MP:{samurai.GetCurrentMp()}/{samurai.GetMaxMp()}");
        }
        else if (target is Monster monster)
        {
            _gameUi.WriteLine($"{targetIndex+1}-{monster.GetName()} HP:{monster.GetCurrentHp()}/{monster.GetMaxHp()} MP:{monster.GetCurrentMp()}/{monster.GetMaxMp()}");
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