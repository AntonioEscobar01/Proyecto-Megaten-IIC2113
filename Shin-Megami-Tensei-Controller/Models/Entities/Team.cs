namespace Shin_Megami_Tensei;

public class Team
{
    private const int MAX_SAMURAI_ALLOWED = 1;
    private const int MAX_ABILITIES_ALLOWED = 8;
    private const int MAX_VISIBLE_MONSTERS = 3;
    
    private int _initialMaxFullTurns;
    private string _player;
    private List<Monster> _units;
    private Samurai? _samurai;
    private List<string>? _originalMonstersOrder;
    private List<IUnit> _orderList;
    private int _fullTurns;
    private int _blinkingTurns;
    private int _maxFullTurns;
    private int _usedSkillsCount;

    public Team(string player)
    {
        _player = player;
        _units = new List<Monster>();
        _orderList = new List<IUnit>();
        _fullTurns = 0;
        _blinkingTurns = 0;
        _usedSkillsCount = 0;
        _originalMonstersOrder = new List<string>();
    }

    public string GetPlayer() => _player;
    public Samurai? GetSamurai() => _samurai;
    public List<Monster> GetUnits() => _units;
    public List<IUnit> GetOrderList() => _orderList;
    public int GetFullTurns() => _fullTurns;
    public int GetBlinkingTurns() => _blinkingTurns;
    public int GetMaxFullTurns() => _maxFullTurns;
    public int GetUsedSkillsCount() => _usedSkillsCount;
    public List<string> GetOriginalMonstersOrder() => new List<string>(_originalMonstersOrder ?? new List<string>());

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

    public IUnit? GetCurrentUnit()
    {
        return _orderList.Count > 0 ? _orderList[0] : null;
    }

    public string GetSamuraiName()
    {
        return _samurai?.GetName() ?? "";
    }

    public bool IsMonsterAliveAtPosition(int position)
    {
        return position < _units.Count && !_units[position].IsDead();
    }

    public Monster GetMonsterAtPosition(int position)
    {
        return position < _units.Count ? _units[position] : null;
    }

    public bool IsPositionValid(int position)
    {
        return position >= 0 && position < _units.Count;
    }

    public void InitializeOrderList()
    {
        _orderList.Clear();
        AddUnitsToOrderList();
        SortOrderListBySpeed();
    }

    private void AddUnitsToOrderList()
    {
        if (_samurai != null)
            _orderList.Add(_samurai);
        for (int i = 0; i < Math.Min(_units.Count, MAX_VISIBLE_MONSTERS); i++)
            _orderList.Add(_units[i]);
    }

    private void SortOrderListBySpeed()
    {
        if (!HasMultipleUnitsInOrder())
            return;
            
        var sortedUnits = CreateSpeedSortedList();
        _orderList = sortedUnits;
    }

    private List<IUnit> CreateSpeedSortedList()
    {
        var sortedUnits = new List<IUnit>();
        var unitsBySpeed = GroupUnitsBySpeed();
        
        foreach (var speedGroup in unitsBySpeed.OrderByDescending(g => g.Key))
        {
            sortedUnits.AddRange(speedGroup.Value);
        }
        
        return sortedUnits;
    }

    private Dictionary<int, List<IUnit>> GroupUnitsBySpeed()
    {
        var unitsBySpeed = new Dictionary<int, List<IUnit>>();
        
        foreach (IUnit unit in _orderList)
        {
            int speed = unit.GetSpd();
            if (!unitsBySpeed.ContainsKey(speed))
            {
                unitsBySpeed[speed] = new List<IUnit>();
            }
            unitsBySpeed[speed].Add(unit);
        }
        
        return unitsBySpeed;
    }

    private bool HasMultipleUnitsInOrder()
    {
        return _orderList.Count > 1;
    }

    public void RotateOrderList()
    {
        if (HasUnitsInOrder())
        {
            var firstUnit = _orderList[0];
            _orderList.RemoveAt(0);
            _orderList.Add(firstUnit);
        }
    }

    private bool HasUnitsInOrder()
    {
        return _orderList.Count > 0;
    }

    public bool IsValidTeam(List<string> possibleUnits)
    {
        _originalMonstersOrder = ExtractMonsterNames(possibleUnits);
        
        if (!IsTeamStructureValid(possibleUnits))
            return false;
            
        BuildTeamFromValidatedUnits(possibleUnits);
        return true;
    }

    private List<string> ExtractMonsterNames(List<string> possibleUnits)
    {
        var monsterNames = new List<string>();
        foreach (string unitName in possibleUnits)
        {
            if (!IsSamuraiUnit(unitName))
            {
                monsterNames.Add(unitName);
            }
        }
        return monsterNames;
    }

    private bool IsTeamStructureValid(List<string> possibleUnits)
    {
        if (HasTooManyUnits(possibleUnits)) 
            return false;

        HashSet<string> uniqueUnits = new HashSet<string>();
        int samuraiCount = 0;

        foreach (var unit in possibleUnits)
        {
            if (IsDuplicateUnit(uniqueUnits, unit)) 
                return false;
    
            if (IsSamuraiUnit(unit))
            {
                samuraiCount++;
                if (!ValidateSamuraiUnit(samuraiCount, unit))
                    return false;
            }
        }
    
        return HasCorrectSamuraiCount(samuraiCount);
    }

    private bool IsSamuraiUnit(string unit)
    {
        return unit.StartsWith("[Samurai]");
    }
    
    private bool ValidateSamuraiUnit(int samuraiCount, string unit)
    {
        if (ExceedsSamuraiLimit(samuraiCount)) 
            return false;
        if (!IsSamuraiDataValid(unit)) 
            return false;
        return true;
    }

    private bool ExceedsSamuraiLimit(int samuraiCount)
    {
        return samuraiCount > MAX_SAMURAI_ALLOWED;
    }
    
    private bool IsSamuraiDataValid(string unit)
    {
        List<string> abilities = ExtractAbilities(unit);
        return HasValidAbilitiesCount(abilities) && HasUniqueAbilities(abilities);
    }

    private bool HasValidAbilitiesCount(List<string> abilities)
    {
        return !HasTooManyAbilities(abilities);
    }

    private bool HasUniqueAbilities(List<string> abilities)
    {
        return !HasDuplicateAbilities(abilities);
    }

    private void BuildTeamFromValidatedUnits(List<string> possibleUnits)
    {
        foreach (var unit in possibleUnits)
        {
            ProcessUnitCreation(unit);
        }
    }

    private void ProcessUnitCreation(string unit)
    {
        if (IsSamuraiUnit(unit))
        {
            CreateAndAddSamurai(unit);
        }
        else 
        {
            AddMonsterToTeam(unit);
        }
    }

    private void CreateAndAddSamurai(string unit)
    {
        List<string> abilities = ExtractAbilities(unit);
        string samuraiName = ExtractSamuraiName(unit);
        _samurai = new Samurai(samuraiName, abilities);
    }

    private string ExtractSamuraiName(string unit)
    {
        string withoutPrefix = unit.Replace("[Samurai]", "");
        string[] parts = withoutPrefix.Split('(');
        return parts[0].Trim();
    }

    private bool HasTooManyUnits(List<string> units)
    {
        return units.Count > MAX_ABILITIES_ALLOWED;
    }

    private bool IsDuplicateUnit(HashSet<string> uniqueUnits, string unit)
    {
        return !uniqueUnits.Add(unit);
    }

    private bool HasCorrectSamuraiCount(int samuraiCount)
    {
        return samuraiCount == MAX_SAMURAI_ALLOWED;
    }

    private bool HasTooManyAbilities(List<string> abilities)
    {
        return abilities.Count > MAX_ABILITIES_ALLOWED;
    }

    private bool HasDuplicateAbilities(List<string> abilities)
    {
        return abilities.Distinct().Count() != abilities.Count;
    }

    private List<string> ExtractAbilities(string unit)
    {
        if (!unit.Contains("("))
            return new List<string>();
            
        string abilitiesSection = ExtractAbilitiesSection(unit);
        string[] rawAbilities = SplitAbilities(abilitiesSection);
        return CleanAbilities(rawAbilities);
    }

    private string ExtractAbilitiesSection(string unit)
    {
        string[] parts = unit.Split('(');
        return parts[1].TrimEnd(')');
    }

    private string[] SplitAbilities(string abilitiesSection)
    {
        return abilitiesSection.Split(',');
    }

    private List<string> CleanAbilities(string[] rawAbilities)
    {
        var cleanedAbilities = new List<string>();
        foreach (string ability in rawAbilities)
        {
            cleanedAbilities.Add(ability.Trim());
        }
        return cleanedAbilities;
    }

    private void AddMonsterToTeam(string unit)
    {
        _units.Add(new Monster(unit));
    }

    public void SetMaxFullTurns()
    {
        if (IsInitialTurnSetup())
        {
            _maxFullTurns = _orderList.Count;
            _initialMaxFullTurns = _maxFullTurns;
        }
        else 
        {
            _maxFullTurns = _initialMaxFullTurns;
        }
    }

    private bool IsInitialTurnSetup()
    {
        return _fullTurns == 0;
    }

    public void ResetTurns()
    {
        _fullTurns = 0;
    }

    public bool HasCompletedAllTurns()
    {
        return HasCompletedAllFullTurns() && HasNoBlinkingTurns();
    }

    private bool HasCompletedAllFullTurns()
    {
        return _fullTurns == _maxFullTurns;
    }

    private bool HasNoBlinkingTurns()
    {
        return _blinkingTurns == 0;
    }

    public bool AreAllUnitsDead()
    {
        return !_orderList.Any(unit => !unit.IsDead());
    }

    public void RemoveDeadUnits()
    {
        _orderList.RemoveAll(unit => unit.IsDead());
    }

    public void ConsumeFullTurn()
    {
        _fullTurns++;
    }

    public void ConsumeBlinkingTurn()
    {
        _blinkingTurns--;
    }

    public void AddBlinkingTurn()
    {
        _blinkingTurns++;
    }

    public void IncrementUsedSkillsCount()
    {
        _usedSkillsCount++;
    }

    public List<Monster> GetAvailableMonstersForSummon()
    {
        List<Monster> availableMonsters = new List<Monster>();
        int frontLineCount = Math.Min(_units.Count, MAX_VISIBLE_MONSTERS);
    
        if (!HasReserveMonsters(frontLineCount))
            return availableMonsters;
    
        CollectAvailableReserveMonsters(availableMonsters, frontLineCount);
        SortMonstersByOriginalOrder(availableMonsters);
    
        return availableMonsters;
    }

    private void CollectAvailableReserveMonsters(List<Monster> availableMonsters, int frontLineCount)
    {
        for (int i = frontLineCount; i < _units.Count; i++)
        {
            var monster = _units[i];
            if (IsAvailableForSummon(monster))
                availableMonsters.Add(monster);
        }
    }

    private void SortMonstersByOriginalOrder(List<Monster> monsters)
    {
        monsters.Sort((monster1, monster2) => 
        {
            int index1 = GetOriginalMonsterIndex(monster1);
            int index2 = GetOriginalMonsterIndex(monster2);
            return index1.CompareTo(index2);
        });
    }

    private int GetOriginalMonsterIndex(Monster monster)
    {
        int originalIndex = _originalMonstersOrder.IndexOf(monster.GetName());
        return originalIndex == -1 ? _originalMonstersOrder.Count : originalIndex;
    }

    private bool HasReserveMonsters(int frontLineCount)
    {
        return _units.Count > frontLineCount;
    }

    private bool IsAvailableForSummon(Monster monster)
    {
        return !monster.IsDead() && monster.GetName() != "Placeholder";
    }

    public void PlaceMonsterInPosition(Monster summonedMonster, int position)
    {
        if (IsSameMonsterAtPosition(summonedMonster, position))
        {
            HandleSameMonsterPlacement(summonedMonster);
            return;
        }

        RemoveMonsterFromCurrentPosition(summonedMonster);

        if (IsPositionEmptyOrDeadMonster(position))
        {
            PlaceMonsterInEmptyPosition(summonedMonster, position);
        }
        else
        {
            PlaceMonsterInOccupiedPosition(summonedMonster, position);
        }
        
        SortReserveMonsters();
    }

    private bool IsSameMonsterAtPosition(Monster summonedMonster, int position)
    {
        return IsValidPosition(position) && _units[position] == summonedMonster;
    }

    private bool IsValidPosition(int position)
    {
        return position < _units.Count;
    }

    private void HandleSameMonsterPlacement(Monster summonedMonster)
    {
        RemoveMonsterFromOrderList(summonedMonster);
        _orderList.Add(summonedMonster);
    }

    private void RemoveMonsterFromCurrentPosition(Monster summonedMonster)
    {
        _units.Remove(summonedMonster);
    }

    private bool IsPositionEmptyOrDeadMonster(int position)
    {
        return IsPositionBeyondUnits(position) || IsPositionWithDeadMonster(position);
    }

    private bool IsPositionBeyondUnits(int position)
    {
        return position >= _units.Count;
    }

    private bool IsPositionWithDeadMonster(int position)
    {
        return IsValidPosition(position) && _units[position].IsDead();
    }

    private void PlaceMonsterInEmptyPosition(Monster summonedMonster, int position)
    {
        if (IsPositionBeyondUnits(position))
        {
            FillGapsWithPlaceholders(position);
            _units.Add(summonedMonster);
        }
        else
        {
            ReplaceDeadMonsterAtPosition(summonedMonster, position);
        }
        
        UpdateOrderListForPlacement(summonedMonster);
    }

    private void FillGapsWithPlaceholders(int targetPosition)
    {
        while (_units.Count < targetPosition)
        {
            Monster placeholder = CreatePlaceholderMonster();
            _units.Add(placeholder);
        }
    }

    private Monster CreatePlaceholderMonster()
    {
        Monster placeholder = new Monster("Placeholder");
        placeholder.TakeDamage(placeholder.GetCurrentHp());
        return placeholder;
    }

    private void ReplaceDeadMonsterAtPosition(Monster summonedMonster, int position)
    {
        Monster deadMonster = _units[position];
        PreserveDeadMonsterIfNeeded(deadMonster);
        _units[position] = summonedMonster;
    }

    private void PreserveDeadMonsterIfNeeded(Monster deadMonster)
    {
        if (ShouldPreserveDeadMonster(deadMonster))
        {
            _units.Add(deadMonster);
        }
    }
    
    private bool ShouldPreserveDeadMonster(Monster deadMonster)
    {
        return deadMonster.IsDead() && deadMonster.GetName() != "Placeholder";
    }

    private void UpdateOrderListForPlacement(Monster summonedMonster)
    {
        RemoveMonsterFromOrderList(summonedMonster);
        _orderList.Add(summonedMonster);
    }

    private void RemoveMonsterFromOrderList(Monster monster)
    {
        if (_orderList.Contains(monster))
            _orderList.Remove(monster);
    }

    private void PlaceMonsterInOccupiedPosition(Monster summonedMonster, int position)
    {
        Monster replacedMonster = _units[position];
        _units[position] = summonedMonster;
        
        UpdateOrderListForReplacement(replacedMonster, summonedMonster);
        EnsureAllFrontlinePositionsExist();
        MoveReplacedMonsterToReserve(replacedMonster);
    }

    private void UpdateOrderListForReplacement(Monster replacedMonster, Monster summonedMonster)
    {
        if (IsMonsterInOrderList(replacedMonster))
        {
            ReplaceMonsterInOrderList(replacedMonster, summonedMonster);
        }
        else if (!IsMonsterInOrderList(summonedMonster))
        {
            _orderList.Add(summonedMonster);
        }
    }
    
    private bool IsMonsterInOrderList(Monster monster)
    {
        return _orderList.Contains(monster);
    }

    private void ReplaceMonsterInOrderList(Monster oldMonster, Monster newMonster)
    {
        int index = _orderList.IndexOf(oldMonster);
        _orderList[index] = newMonster;
    }

    private void EnsureAllFrontlinePositionsExist()
    {
        for (int i = 0; i < MAX_VISIBLE_MONSTERS; i++)
        {
            if (NeedsPlaceholderAtPosition(i))
            {
                Monster placeholder = CreatePlaceholderMonster();
                _units.Add(placeholder);
            }
        }
    }
    
    private bool NeedsPlaceholderAtPosition(int position)
    {
        return _units.Count <= position;
    }

    private void MoveReplacedMonsterToReserve(Monster replacedMonster)
    {
        _units.Add(replacedMonster);
    }

    private void SortReserveMonsters()
    {
        if (!HasEnoughUnitsForReserve())
            return;

        var reserveMonsters = ExtractReserveMonsters();
        SortReserveMonstersByPriority(reserveMonsters);
        ReplaceReserveMonsters(reserveMonsters);
    }

    private List<Monster> ExtractReserveMonsters()
    {
        return _units.Skip(MAX_VISIBLE_MONSTERS).ToList();
    }

    private void SortReserveMonstersByPriority(List<Monster> reserveMonsters)
    {
        reserveMonsters.Sort((monster1, monster2) => 
        {
            int priority1 = GetMonsterSortPriority(monster1);
            int priority2 = GetMonsterSortPriority(monster2);
            return priority1.CompareTo(priority2);
        });
    }

    private int GetMonsterSortPriority(Monster monster)
    {
        int originalIndex = _originalMonstersOrder.IndexOf(monster.GetName());
        
        if (originalIndex != -1)
            return originalIndex;
        
        if (IsPlaceholderMonster(monster))
            return int.MaxValue;
            
        return _originalMonstersOrder.Count + monster.GetName().GetHashCode() % 1000;
    }

    private void ReplaceReserveMonsters(List<Monster> sortedReserveMonsters)
    {
        int reserveStartIndex = MAX_VISIBLE_MONSTERS;
        int reserveCount = _units.Count - reserveStartIndex;
        
        _units.RemoveRange(reserveStartIndex, reserveCount);
        _units.AddRange(sortedReserveMonsters);
    }

    private bool HasEnoughUnitsForReserve()
    {
        return _units.Count > MAX_VISIBLE_MONSTERS;
    }
    
    private bool IsPlaceholderMonster(Monster monster)
    {
        return monster.GetName() == "Placeholder";
    }

    public void SwapMonsters(Monster currentMonster, Monster summonedMonster)
    {
        int currentIndex = _units.IndexOf(currentMonster);
        int summonedIndex = _units.IndexOf(summonedMonster);
        
        if (AreIndicesValid(currentIndex, summonedIndex))
        {
            _units[currentIndex] = summonedMonster;
            _units[summonedIndex] = currentMonster;
        }
        
        int orderIndex = _orderList.IndexOf(currentMonster);
        if (IsValidOrderIndex(orderIndex))
            _orderList[orderIndex] = summonedMonster;
        
        SortReserveMonsters();
    }

    private bool AreIndicesValid(int currentIndex, int summonedIndex)
    {
        return currentIndex >= 0 && summonedIndex >= 0;
    }
    
    private bool IsValidOrderIndex(int orderIndex)
    {
        return orderIndex >= 0;
    }

    public void ConsumeSummonTurns()
    {
        if (HasBlinkingTurns())
            ConsumeBlinkingTurn();
        else
        {
            ConsumeFullTurn();
            AddBlinkingTurn();
        }
        RotateOrderList();
    }
    
    private bool HasBlinkingTurns()
    {
        return _blinkingTurns > 0;
    }
    
    public void ConsumeNonOffensiveSkillsTurns()
    {
        if (HasBlinkingTurns())
            ConsumeBlinkingTurn();
        else
        {
            ConsumeFullTurn();
        }
        RotateOrderList();
    }
}