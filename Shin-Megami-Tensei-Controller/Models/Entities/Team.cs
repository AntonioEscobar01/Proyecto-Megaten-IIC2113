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
        if (OrderList.Count > 1)
            OrderList = OrderList
                .OrderByDescending(unit => unit is Monster monster ? monster.Spd : 
                    unit is Samurai samurai ? samurai.Spd : 0)
                .ToList();
    }

    public void RotateOrderList()
    {
        if (OrderList.Count > 0)
        {
            var firstUnit = OrderList[0];
            OrderList.RemoveAt(0);
            OrderList.Add(firstUnit);
        }
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
    
            if (unit.StartsWith("[Samurai]"))
            {
                samuraiCount++;
                if (!ValidateSamuraiUnit(samuraiCount, unit))
                    return false;
            }
        }
    
        return HasCorrectSamuraiCount(samuraiCount);
    }
    
    private bool ValidateSamuraiUnit(int samuraiCount, string unit)
    {
        if (DoesSamuraiCountExceedLimit(samuraiCount)) 
            return false;
        if (!IsSamuraiDataValid(unit)) 
            return false;
        return true;
    }
    
    private bool IsSamuraiDataValid(string unit)
    {
        List<string> abilities = ExtractAbilities(unit);
        return !HasTooManyAbilities(abilities) && !HasDuplicateAbilities(abilities);
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
        if (unit.StartsWith("[Samurai]"))
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

    private bool DoesSamuraiCountExceedLimit(int samuraiCount)
    {
        return samuraiCount > MAX_SAMURAI_ALLOWED;
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

    // ... resto de métodos sin cambios
    public void SetMaxFullTurns()
    {
        if (FullTurns == 0)
        {
            MaxFullTurns = OrderList.Count;
            _initialMaxFullTurns = MaxFullTurns;
        }
        else 
        {
            MaxFullTurns = _initialMaxFullTurns;
        }
    }

    public void ResetTurns()
    {
        FullTurns = 0;
    }

    public bool HasCompletedAllTurns()
    {
        return FullTurns == MaxFullTurns && BlinkingTurns == 0;
    }

    public bool AreAllUnitsDead()
    {
        return !OrderList.Any(unit => (unit is Monster monster && !monster.IsDead()) || (unit is Samurai samurai && !samurai.IsDead()));
    }

    public void RemoveDeadUnits()
    {
        OrderList.RemoveAll(unit => 
        {
            if (unit is Monster monster)
            {
                return monster.IsDead();
            }
            else if (unit is Samurai samurai)
            {
                return samurai.IsDead();
            }
            return false;
        });
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
    
        if (Units.Count <= frontLineCount)
            return availableMonsters;
    
        for (int i = frontLineCount; i < Units.Count; i++)
        {
            var monster = Units[i];
            if (!monster.IsDead() && monster.Name != "Placeholder")
                availableMonsters.Add(monster);
        }
    
        availableMonsters = availableMonsters.OrderBy(monster => 
        {
            int originalIndex = _originalMonstersOrder.IndexOf(monster.Name);
            return originalIndex == -1 ? _originalMonstersOrder.Count : originalIndex;
        }).ToList();
    
        return availableMonsters;
    }
    public void PlaceMonsterInPosition(Monster summonedMonster, int position)
    {
        if (IsSameMonsterAtPosition(summonedMonster, position))
        {
            HandleSameMonsterPlacement(summonedMonster);
            return;
        }

        RemoveMonsterFromCurrentPosition(summonedMonster);

        if (IsPositionEmpty(position))
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
        return position < Units.Count && Units[position] == summonedMonster;
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

    private bool IsPositionEmpty(int position)
    {
        return position >= Units.Count || (position < Units.Count && Units[position].IsDead());
    }

    private void PlaceMonsterInEmptyPosition(Monster summonedMonster, int position)
    {
        if (position >= Units.Count)
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
        if (deadMonster.IsDead() && deadMonster.Name != "Placeholder")
        {
            Units.Add(deadMonster);
        }
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
        if (OrderList.Contains(replacedMonster))
        {
            ReplaceMonsterInOrderList(replacedMonster, summonedMonster);
        }
        else if (!OrderList.Contains(summonedMonster))
        {
            OrderList.Add(summonedMonster);
        }
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
            if (Units.Count <= i)
            {
                Monster placeholder = CreatePlaceholderMonster();
                Units.Add(placeholder);
            }
        }
    }

    private void MoveReplacedMonsterToReserve(Monster replacedMonster)
    {
        Units.Add(replacedMonster);
    }

    private void SortReserveMonsters()
    {
        if (Units.Count <= MAX_VISIBLE_MONSTERS)
            return;
        var reserveMonsters = Units.Skip(MAX_VISIBLE_MONSTERS).ToList();
        reserveMonsters = reserveMonsters.OrderBy(monster => 
        {
            int originalIndex = _originalMonstersOrder.IndexOf(monster.Name);
            if (originalIndex == -1)
            {
                if (monster.Name == "Placeholder")
                    return int.MaxValue;
                return _originalMonstersOrder.Count + monster.Name.GetHashCode() % 1000;
            }
            return originalIndex;
        }).ToList();
        Units.RemoveRange(MAX_VISIBLE_MONSTERS, Units.Count - MAX_VISIBLE_MONSTERS);
        Units.AddRange(reserveMonsters);
    }
    public void SwapMonsters(Monster currentMonster, Monster summonedMonster)
    {
        int currentIndex = Units.IndexOf(currentMonster);
        int summonedIndex = Units.IndexOf(summonedMonster);
        if (currentIndex >= 0 && summonedIndex >= 0)
        {
            Units[currentIndex] = summonedMonster;
            Units[summonedIndex] = currentMonster;
        }
        int orderIndex = OrderList.IndexOf(currentMonster);
        if (orderIndex >= 0)
            OrderList[orderIndex] = summonedMonster;
        SortReserveMonsters();
    }
    public void ConsumeSummonTurns()
    {
        if (BlinkingTurns > 0)
            ConsumeBlinkingTurn();
        else
        {
            ConsumeFullTurn();
            AddBlinkingTurn();
        }
        RotateOrderList();
    }
    
    public void ConsumeNonOffensiveSkillsTurns()
    {
        if (BlinkingTurns > 0)
            ConsumeBlinkingTurn();
        else
        {
            ConsumeFullTurn();
        }
        RotateOrderList();
    }
}