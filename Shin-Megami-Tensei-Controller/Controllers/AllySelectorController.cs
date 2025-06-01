namespace Shin_Megami_Tensei;

public class AllySelectorController
{
    private readonly GameUi _gameUi;
    private readonly Team _allyTeam;
    private List<IUnit> _availableAllies;

    public AllySelectorController(GameUi gameUi, Team allyTeam)
    {
        _gameUi = gameUi;
        _allyTeam = allyTeam;
        _availableAllies = new List<IUnit>();
    }

    public int ChooseAllyToHeal(IUnit healer)
    {
        string healerName = healer.Name;
        _gameUi.WriteLine($"Seleccione un objetivo para {healerName}");

        _availableAllies = GetAvailableAlliesForHealing();
        DisplayAllies();

        return GetAllySelectionResult();
    }
    
    public int ChooseAllyToRevive(IUnit healer)
    {
        string healerName = healer.Name;
        _gameUi.WriteLine($"Seleccione un objetivo para {healerName}");

        _availableAllies = GetAvailableAlliesForReviving();
        DisplayAllies();

        return GetAllySelectionResult();
    }
    
    private List<IUnit> GetAvailableAlliesForHealing()
    {
        List<IUnit> allies = new List<IUnit>();
    
        AddSamuraiIfAlive(allies);
        AddLivingMonstersToList(allies);
    
        return allies;
    }
    
    private List<IUnit> GetAvailableAlliesForReviving()
    {
        List<IUnit> allies = new List<IUnit>();
    
        AddSamuraiIfDead(allies);
        AddDeadMonstersToList(allies);
    
        return allies;
    }
    
    private void AddDeadMonstersToList(List<IUnit> allies)
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
        return IsValidMonster(monster) && IsMonsterAlive(monster);
    }

    private bool ShouldIncludeDeadMonster(Monster monster)
    {
        return IsValidMonster(monster) && IsMonsterDead(monster);
    }

    private bool IsValidMonster(Monster monster)
    {
        return monster.Name != "Placeholder";
    }

    private bool IsMonsterAlive(Monster monster)
    {
        return !monster.IsDead();
    }

    private bool IsMonsterDead(Monster monster)
    {
        return monster.IsDead();
    }
    
    private void AddSamuraiIfAlive(List<IUnit> allies)
    {
        if (IsSamuraiAliveAndAvailable())
        {
            allies.Add(_allyTeam.Samurai);
        }
    }

    private void AddSamuraiIfDead(List<IUnit> allies)
    {
        if (IsSamuraiDeadAndAvailable())
        {
            allies.Add(_allyTeam.Samurai);
        }
    }

    private bool IsSamuraiAliveAndAvailable()
    {
        return _allyTeam.Samurai != null && !_allyTeam.Samurai.IsDead();
    }

    private bool IsSamuraiDeadAndAvailable()
    {
        return _allyTeam.Samurai != null && _allyTeam.Samurai.IsDead();
    }

    private void AddLivingMonstersToList(List<IUnit> allies)
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

    private void DisplayAlly(IUnit ally, int allyIndex)
    {
        if (ally is Samurai samurai)
        {
            DisplaySamuraiInfo(samurai, allyIndex);
        }
        else if (ally is Monster monster)
        {
            DisplayMonsterInfo(monster, allyIndex);
        }
    }

    private void DisplaySamuraiInfo(Samurai samurai, int allyIndex)
    {
        _gameUi.WriteLine($"{allyIndex+1}-{samurai.Name} HP:{samurai.Hp}/{samurai.OriginalHp} MP:{samurai.Mp}/{samurai.OriginalMp}");
    }

    private void DisplayMonsterInfo(Monster monster, int allyIndex)
    {
        _gameUi.WriteLine($"{allyIndex+1}-{monster.Name} HP:{monster.Hp}/{monster.OriginalHp} MP:{monster.Mp}/{monster.OriginalMp}");
    }

    private int GetAllySelectionResult()
    {
        int selection = int.Parse(_gameUi.ReadLine());
        
        if (IsCancelSelection(selection))
            return ActionConstantsData.CancelTargetSelection;
            
        if (IsValidAllySelection(selection))
            return selection;
        
        return ActionConstantsData.CancelTargetSelection;
    }

    private bool IsCancelSelection(int selection)
    {
        return selection == _availableAllies.Count + 1;
    }
    
    private bool IsValidAllySelection(int selection)
    {
        return selection > 0 && selection <= _availableAllies.Count;
    }

    public IUnit GetAlly(int selection)
    {
        if (IsValidAllySelection(selection))
            return _availableAllies[selection - 1];
        
        return null;
    }
}