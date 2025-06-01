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

    public int ChooseAllyToHeal(object healer)
    {
        string healerName = _gameUi.GetUnitName(healer);
        _gameUi.WriteLine($"Seleccione un objetivo para {healerName}");

        _availableAllies = GetAvailableAlliesForHealing();
        DisplayAllies();

        return GetAllySelectionResult();
    }
    
    public int ChooseAllyToRevive(object healer)
    {
        string healerName = _gameUi.GetUnitName(healer);
        _gameUi.WriteLine($"Seleccione un objetivo para {healerName}");

        _availableAllies = GetAvailableAlliesForReviving();
        DisplayAllies();

        return GetAllySelectionResult();
    }
    
    private List<object> GetAvailableAlliesForHealing()
    {
        List<object> allies = new List<object>();
    
        AddSamuraiIfAlive(allies);
        AddLivingMonstersToList(allies);
    
        return allies;
    }
    
    private List<object> GetAvailableAlliesForReviving()
    {
        List<object> allies = new List<object>();
    
        AddSamuraiIfDead(allies);
        AddDeadMonstersToList(allies);
    
        return allies;
    }
    
    private void AddDeadMonstersToList(List<object> allies)
    {
        int maxMonsters = _allyTeam.Units.Count;
    
        for (int i = 0; i < maxMonsters; i++)
        {
            var monster = _allyTeam.Units[i];
            if (ShouldIncludeDeadMonster(monster))
                allies.Add(monster);
        }
    }

    private bool ShouldIncludeLivingMonster(Monster monster)
    {
        return monster.Name != "Placeholder" && !monster.IsDead();
    }

    private bool ShouldIncludeDeadMonster(Monster monster)
    {
        return monster.Name != "Placeholder" && monster.IsDead();
    }
    
    private void AddSamuraiIfAlive(List<object> allies)
    {
        if (_allyTeam.Samurai != null && !_allyTeam.Samurai.IsDead())
        {
            allies.Add(_allyTeam.Samurai);
        }
    }

    private void AddSamuraiIfDead(List<object> allies)
    {
        if (_allyTeam.Samurai != null && _allyTeam.Samurai.IsDead())
        {
            allies.Add(_allyTeam.Samurai);
        }
    }

    private void AddLivingMonstersToList(List<object> allies)
    {
        int maxMonsters = Math.Min(_allyTeam.Units.Count, 3);
    
        for (int i = 0; i < maxMonsters; i++)
        {
            var monster = _allyTeam.Units[i];
            if (ShouldIncludeLivingMonster(monster))
                allies.Add(monster);
        }
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