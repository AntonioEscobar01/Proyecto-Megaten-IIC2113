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
        string attackerName = attacker.Name;
        _gameUi.WriteLine($"Seleccione un objetivo para {attackerName}");

        _availableTargets = GetAvailableTargets();
        DisplayTargets();

        return GetTargetSelectionResult();
    }

    private List<IUnit> GetAvailableTargets()
    {
        List<IUnit> targets = new List<IUnit>();
        
        if (_enemyTeam.Samurai != null && !_enemyTeam.Samurai.IsDead())
        {
            targets.Add(_enemyTeam.Samurai);
        }

        AddAvailableMonsters(targets);
        return targets;
    }

    private void AddAvailableMonsters(List<IUnit> targets)
    {
        int maxVisibleMonsters = Math.Min(_enemyTeam.Units.Count, 3);
    
        for (int monsterIndex = 0; monsterIndex < maxVisibleMonsters; monsterIndex++)
        {
            var monster = _enemyTeam.Units[monsterIndex];
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
            _gameUi.WriteLine($"{targetIndex+1}-{samurai.Name} HP:{samurai.Hp}/{samurai.OriginalHp} MP:{samurai.Mp}/{samurai.OriginalMp}");
        }
        else if (target is Monster monster)
        {
            _gameUi.WriteLine($"{targetIndex+1}-{monster.Name} HP:{monster.Hp}/{monster.OriginalHp} MP:{monster.Mp}/{monster.OriginalMp}");
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
            return _enemyTeam.Units.IndexOf((Monster)selectedTarget) + 2;
    }

    public IUnit GetTarget(int targetIndex)
    {
        return targetIndex switch
        {
            1 => _enemyTeam.Samurai,
            var idx when idx > 1 && idx <= _enemyTeam.Units.Count + 1 => _enemyTeam.Units[targetIndex - 2],
            _ => null
        };
    }
}