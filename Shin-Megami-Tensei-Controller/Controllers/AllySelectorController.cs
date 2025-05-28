namespace Shin_Megami_Tensei;

public class AllySelectorController
{
    private readonly GameUi _gameUi;
    private readonly Team _allyTeam;
    private List<object> _availableAllies;

    public AllySelectorController(GameUi gameUi, Team allyTeam)
    {
        _gameUi = gameUi;
        _allyTeam = allyTeam;
        _availableAllies = new List<object>();
    }

    public int ChooseAllyToHeal(object healer, bool reviveMode = false)
    {
        string healerName = _gameUi.GetUnitName(healer);
        _gameUi.WriteLine($"Seleccione un objetivo para {healerName}");

        _availableAllies = GetAvailableAllies(reviveMode);
        DisplayAllies();

        return GetAllySelectionResult();
    }

    private List<object> GetAvailableAllies(bool reviveMode)
    {
        List<object> allies = new List<object>();
        if (_allyTeam.Samurai != null)
        {
            bool shouldInclude = reviveMode ? _allyTeam.Samurai.IsDead() : !_allyTeam.Samurai.IsDead();
            if (shouldInclude)
            {
                allies.Add(_allyTeam.Samurai);
            }
        }
        
        int maxMonsters = reviveMode ? _allyTeam.Units.Count : Math.Min(_allyTeam.Units.Count, 3);
    
        for (int i = 0; i < maxMonsters; i++)
        {
            var monster = _allyTeam.Units[i];
            
            if (monster.Name == "Placeholder")
                continue;
            
            bool shouldInclude = reviveMode ? monster.IsDead() : !monster.IsDead();
            if (shouldInclude)
            {
                allies.Add(monster);
            }
        }
        return allies;
    }

    private void DisplayAllies()
    {
        for (int allyIndex = 0; allyIndex < _availableAllies.Count; allyIndex++)
        {
            DisplayAlly(_availableAllies[allyIndex], allyIndex);
        }
        _gameUi.WriteLine($"{_availableAllies.Count + 1}-Cancelar");
    }

    private void DisplayAlly(object ally, int allyIndex)
    {
        if (ally is Samurai samurai)
        {
            _gameUi.WriteLine($"{allyIndex+1}-{samurai.Name} HP:{samurai.Hp}/{samurai.OriginalHp} MP:{samurai.Mp}/{samurai.OriginalMp}");
        }
        else if (ally is Monster monster)
        {
            _gameUi.WriteLine($"{allyIndex+1}-{monster.Name} HP:{monster.Hp}/{monster.OriginalHp} MP:{monster.Mp}/{monster.OriginalMp}");
        }
    }

    private int GetAllySelectionResult()
    {
        int selection = int.Parse(_gameUi.ReadLine());
        
        if (selection == _availableAllies.Count + 1)
            return ActionConstantsData.CancelTargetSelection;
            
        if (selection > 0 && selection <= _availableAllies.Count)
            return selection;
        
        return ActionConstantsData.CancelTargetSelection;
    }

    public object GetAlly(int selection)
    {
        if (selection > 0 && selection <= _availableAllies.Count)
            return _availableAllies[selection - 1];
        
        return null;
    }
}