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
        return IsValidMonster(monster) && IsMonsterAlive(monster);
    }

    private bool ShouldIncludeDeadMonster(Monster monster)
    {
        return IsValidMonster(monster) && IsMonsterDead(monster);
    }

    // ✅ CONDICIÓN ENCAPSULADA: Validación de monstruo válido (no placeholder)
    private bool IsValidMonster(Monster monster)
    {
        return monster.Name != "Placeholder";
    }

    // ✅ CONDICIÓN ENCAPSULADA: Validación de monstruo vivo
    private bool IsMonsterAlive(Monster monster)
    {
        return !monster.IsDead();
    }

    // ✅ CONDICIÓN ENCAPSULADA: Validación de monstruo muerto
    private bool IsMonsterDead(Monster monster)
    {
        return monster.IsDead();
    }
    
    private void AddSamuraiIfAlive(List<object> allies)
    {
        if (IsSamuraiAliveAndAvailable())
        {
            allies.Add(_allyTeam.Samurai);
        }
    }

    private void AddSamuraiIfDead(List<object> allies)
    {
        if (IsSamuraiDeadAndAvailable())
        {
            allies.Add(_allyTeam.Samurai);
        }
    }

    // ✅ CONDICIÓN ENCAPSULADA: Validación de samurai vivo y disponible
    private bool IsSamuraiAliveAndAvailable()
    {
        return _allyTeam.Samurai != null && !_allyTeam.Samurai.IsDead();
    }

    // ✅ CONDICIÓN ENCAPSULADA: Validación de samurai muerto y disponible
    private bool IsSamuraiDeadAndAvailable()
    {
        return _allyTeam.Samurai != null && _allyTeam.Samurai.IsDead();
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

    public object GetAlly(int selection)
    {
        if (IsValidAllySelection(selection))
            return _availableAllies[selection - 1];
        
        return null;
    }
}