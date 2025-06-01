namespace Shin_Megami_Tensei;

public class Team
{
    private const int MAX_SAMURAI_ALLOWED = 1;
    private const int MAX_ABILITIES_ALLOWED = 8;
    private const int MAX_VISIBLE_MONSTERS = 3;
    private int _initialMaxFullTurns;
    public string Player { get; private set; }
    public List<Monster> Units { get; private set; }
    public Samurai? Samurai { get; private set; }
    public List<string> _originalMonstersOrder;
    public List<IUnit> OrderList { get; private set; }
    public int FullTurns { get; private set; }
    public int BlinkingTurns { get; private set; }
    public int MaxFullTurns { get; private set; } 
    public int UsedSkillsCount { get; private set; }

    public Team(string player)
    {
        Player = player;
        Units = new List<Monster>();
        OrderList = new List<IUnit>();
        FullTurns = 0;
        BlinkingTurns = 0;
        UsedSkillsCount = 0;
    }

    public void InitializeOrderList()
    {
        OrderList.Clear();
        AddUnitsToOrderList();
        SortOrderListBySpeed();
    }

    private void AddUnitsToOrderList()
    {
        if (Samurai != null)
            OrderList.Add(Samurai);
        for (int i = 0; i < Math.Min(Units.Count, MAX_VISIBLE_MONSTERS); i++)
            OrderList.Add(Units[i]);
    }

    private void SortOrderListBySpeed()
    {
        if (HasMultipleUnitsInOrder())
            OrderList = OrderList
                .OrderByDescending(unit => unit.Spd)
                .ToList();
    }

    private bool HasMultipleUnitsInOrder()
    {
        return OrderList.Count > 1;
    }

    public void RotateOrderList()
    {
        if (HasUnitsInOrder())
        {
            var firstUnit = OrderList[0];
            OrderList.RemoveAt(0);
            OrderList.Add(firstUnit);
        }
    }

    private bool HasUnitsInOrder()
    {
        return OrderList.Count > 0;
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
        return possibleUnits
            .Where(name => !name.Contains("[Samurai]"))
            .ToList();
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
        var samuraiName = unit.Replace("[Samurai]", "").Split('(')[0].Trim();
        Samurai = new Samurai(samuraiName, abilities);
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
        if (unit.Contains("("))
            return unit.Split('(')[1].TrimEnd(')').Split(',').Select(h => h.Trim()).ToList();
    
        return new List<string>();
    }

    private void AddMonsterToTeam(string unit)
    {
        Units.Add(new Monster(unit));
    }

    public void SetMaxFullTurns()
    {
        if (IsInitialTurnSetup())
        {
            MaxFullTurns = OrderList.Count;
            _initialMaxFullTurns = MaxFullTurns;
        }
        else 
        {
            MaxFullTurns = _initialMaxFullTurns;
        }
    }

    private bool IsInitialTurnSetup()
    {
        return FullTurns == 0;
    }

    public void ResetTurns()
    {
        FullTurns = 0;
    }

    public bool HasCompletedAllTurns()
    {
        return HasCompletedAllFullTurns() && HasNoBlinkingTurns();
    }

    private bool HasCompletedAllFullTurns()
    {
        return FullTurns == MaxFullTurns;
    }

    private bool HasNoBlinkingTurns()
    {
        return BlinkingTurns == 0;
    }

    public bool AreAllUnitsDead()
    {
        return !OrderList.Any(unit => !unit.IsDead());
    }

    public void RemoveDeadUnits()
    {
        OrderList.RemoveAll(unit => unit.IsDead());
    }

    public void ConsumeFullTurn()
    {
        FullTurns++;
    }

    public void ConsumeBlinkingTurn()
    {
        BlinkingTurns--;
    }

    public void AddBlinkingTurn()
    {
        BlinkingTurns++;
    }

    public void IncrementUsedSkillsCount()
    {
        UsedSkillsCount++;
    }

    public List<Monster> GetAvailableMonstersForSummon()
    {
        List<Monster> availableMonsters = new List<Monster>();
        int frontLineCount = Math.Min(Units.Count, MAX_VISIBLE_MONSTERS);
    
        if (!HasReserveMonsters(frontLineCount))
            return availableMonsters;
    
        for (int i = frontLineCount; i < Units.Count; i++)
        {
            var monster = Units[i];
            if (IsAvailableForSummon(monster))
                availableMonsters.Add(monster);
        }
    
        availableMonsters = availableMonsters.OrderBy(monster => 
        {
            int originalIndex = _originalMonstersOrder.IndexOf(monster.Name);
            return originalIndex == -1 ? _originalMonstersOrder.Count : originalIndex;
        }).ToList();
    
        return availableMonsters;
    }

    private bool HasReserveMonsters(int frontLineCount)
    {
        return Units.Count > frontLineCount;
    }

    private bool IsAvailableForSummon(Monster monster)
    {
        return !monster.IsDead() && monster.Name != "Placeholder";
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
        return IsValidPosition(position) && Units[position] == summonedMonster;
    }

    private bool IsValidPosition(int position)
    {
        return position < Units.Count;
    }

    private void HandleSameMonsterPlacement(Monster summonedMonster)
    {
        RemoveMonsterFromOrderList(summonedMonster);
        OrderList.Add(summonedMonster);
    }

    private void RemoveMonsterFromCurrentPosition(Monster summonedMonster)
    {
        Units.Remove(summonedMonster);
    }

    private bool IsPositionEmptyOrDeadMonster(int position)
    {
        return IsPositionBeyondUnits(position) || IsPositionWithDeadMonster(position);
    }

    private bool IsPositionBeyondUnits(int position)
    {
        return position >= Units.Count;
    }

    private bool IsPositionWithDeadMonster(int position)
    {
        return IsValidPosition(position) && Units[position].IsDead();
    }

    private void PlaceMonsterInEmptyPosition(Monster summonedMonster, int position)
    {
        if (IsPositionBeyondUnits(position))
        {
            FillGapsWithPlaceholders(position);
            Units.Add(summonedMonster);
        }
        else
        {
            ReplaceDeadMonsterAtPosition(summonedMonster, position);
        }
        
        UpdateOrderListForPlacement(summonedMonster);
    }

    private void FillGapsWithPlaceholders(int targetPosition)
    {
        while (Units.Count < targetPosition)
        {
            Monster placeholder = CreatePlaceholderMonster();
            Units.Add(placeholder);
        }
    }

    private Monster CreatePlaceholderMonster()
    {
        Monster placeholder = new Monster("Placeholder");
        placeholder.Hp = 0;
        return placeholder;
    }

    private void ReplaceDeadMonsterAtPosition(Monster summonedMonster, int position)
    {
        Monster deadMonster = Units[position];
        PreserveDeadMonsterIfNeeded(deadMonster);
        Units[position] = summonedMonster;
    }

    private void PreserveDeadMonsterIfNeeded(Monster deadMonster)
    {
        if (ShouldPreserveDeadMonster(deadMonster))
        {
            Units.Add(deadMonster);
        }
    }
    
    private bool ShouldPreserveDeadMonster(Monster deadMonster)
    {
        return deadMonster.IsDead() && deadMonster.Name != "Placeholder";
    }

    private void UpdateOrderListForPlacement(Monster summonedMonster)
    {
        RemoveMonsterFromOrderList(summonedMonster);
        OrderList.Add(summonedMonster);
    }

    private void RemoveMonsterFromOrderList(Monster monster)
    {
        if (OrderList.Contains(monster))
            OrderList.Remove(monster);
    }

    private void PlaceMonsterInOccupiedPosition(Monster summonedMonster, int position)
    {
        Monster replacedMonster = Units[position];
        Units[position] = summonedMonster;
        
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
            OrderList.Add(summonedMonster);
        }
    }
    
    private bool IsMonsterInOrderList(Monster monster)
    {
        return OrderList.Contains(monster);
    }

    private void ReplaceMonsterInOrderList(Monster oldMonster, Monster newMonster)
    {
        int index = OrderList.IndexOf(oldMonster);
        OrderList[index] = newMonster;
    }

    private void EnsureAllFrontlinePositionsExist()
    {
        for (int i = 0; i < MAX_VISIBLE_MONSTERS; i++)
        {
            if (NeedsPlaceholderAtPosition(i))
            {
                Monster placeholder = CreatePlaceholderMonster();
                Units.Add(placeholder);
            }
        }
    }
    
    private bool NeedsPlaceholderAtPosition(int position)
    {
        return Units.Count <= position;
    }

    private void MoveReplacedMonsterToReserve(Monster replacedMonster)
    {
        Units.Add(replacedMonster);
    }

    private void SortReserveMonsters()
    {
        if (!HasEnoughUnitsForReserve())
            return;

        var reserveMonsters = Units.Skip(MAX_VISIBLE_MONSTERS).ToList();
        reserveMonsters = reserveMonsters.OrderBy(monster => 
        {
            int originalIndex = _originalMonstersOrder.IndexOf(monster.Name);
            if (originalIndex == -1)
            {
                if (IsPlaceholderMonster(monster))
                    return int.MaxValue;
                return _originalMonstersOrder.Count + monster.Name.GetHashCode() % 1000;
            }
            return originalIndex;
        }).ToList();
        Units.RemoveRange(MAX_VISIBLE_MONSTERS, Units.Count - MAX_VISIBLE_MONSTERS);
        Units.AddRange(reserveMonsters);
    }

    private bool HasEnoughUnitsForReserve()
    {
        return Units.Count > MAX_VISIBLE_MONSTERS;
    }
    
    private bool IsPlaceholderMonster(Monster monster)
    {
        return monster.Name == "Placeholder";
    }

    public void SwapMonsters(Monster currentMonster, Monster summonedMonster)
    {
        int currentIndex = Units.IndexOf(currentMonster);
        int summonedIndex = Units.IndexOf(summonedMonster);
        
        if (AreIndicesValid(currentIndex, summonedIndex))
        {
            Units[currentIndex] = summonedMonster;
            Units[summonedIndex] = currentMonster;
        }
        
        int orderIndex = OrderList.IndexOf(currentMonster);
        if (IsValidOrderIndex(orderIndex))
            OrderList[orderIndex] = summonedMonster;
        
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
        return BlinkingTurns > 0;
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