namespace Shin_Megami_Tensei;

public class MonsterPositionManager
{
    private const int MAX_VISIBLE_MONSTERS = 3;
    private List<Monster> _units;
    private List<string> _originalMonstersOrder;

    public MonsterPositionManager()
    {
        _units = new List<Monster>();
        _originalMonstersOrder = new List<string>();
    }

    public List<Monster> GetUnits() => _units;
    public List<string> GetOriginalMonstersOrder() => new List<string>(_originalMonstersOrder);

    public void SetOriginalMonstersOrder(List<string> order)
    {
        _originalMonstersOrder = new List<string>(order);
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

    public void AddMonster(Monster monster)
    {
        _units.Add(monster);
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

    public void PlaceMonsterInPosition(Monster summonedMonster, int position, BattleOrderManager orderManager)
    {
        if (IsSameMonsterAtPosition(summonedMonster, position))
        {
            HandleSameMonsterPlacement(summonedMonster, orderManager);
            return;
        }

        RemoveMonsterFromCurrentPosition(summonedMonster);

        if (IsPositionEmptyOrDeadMonster(position))
        {
            PlaceMonsterInEmptyPosition(summonedMonster, position, orderManager);
        }
        else
        {
            PlaceMonsterInOccupiedPosition(summonedMonster, position, orderManager);
        }
        
        SortReserveMonsters();
    }

    public void SwapMonsters(Monster currentMonster, Monster summonedMonster, BattleOrderManager orderManager)
    {
        int currentIndex = _units.IndexOf(currentMonster);
        int summonedIndex = _units.IndexOf(summonedMonster);
        
        if (AreIndicesValid(currentIndex, summonedIndex))
        {
            _units[currentIndex] = summonedMonster;
            _units[summonedIndex] = currentMonster;
        }
        
        if (orderManager.IsUnitInOrderList(currentMonster))
            orderManager.ReplaceUnitInOrderList(currentMonster, summonedMonster);
        
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

    private void HandleSameMonsterPlacement(Monster summonedMonster, BattleOrderManager orderManager)
    {
        orderManager.RemoveUnitFromOrderList(summonedMonster);
        orderManager.AddUnitToOrderList(summonedMonster);
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

    private void PlaceMonsterInEmptyPosition(Monster summonedMonster, int position, BattleOrderManager orderManager)
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
        
        UpdateOrderListForPlacement(summonedMonster, orderManager);
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

    private void UpdateOrderListForPlacement(Monster summonedMonster, BattleOrderManager orderManager)
    {
        orderManager.RemoveUnitFromOrderList(summonedMonster);
        orderManager.AddUnitToOrderList(summonedMonster);
    }

    private void PlaceMonsterInOccupiedPosition(Monster summonedMonster, int position, BattleOrderManager orderManager)
    {
        Monster replacedMonster = _units[position];
        _units[position] = summonedMonster;
        
        UpdateOrderListForReplacement(replacedMonster, summonedMonster, orderManager);
        EnsureAllFrontlinePositionsExist();
        MoveReplacedMonsterToReserve(replacedMonster);
    }

    private void UpdateOrderListForReplacement(Monster replacedMonster, Monster summonedMonster, BattleOrderManager orderManager)
    {
        if (orderManager.IsUnitInOrderList(replacedMonster))
        {
            orderManager.ReplaceUnitInOrderList(replacedMonster, summonedMonster);
        }
        else if (!orderManager.IsUnitInOrderList(summonedMonster))
        {
            orderManager.AddUnitToOrderList(summonedMonster);
        }
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

    private bool AreIndicesValid(int currentIndex, int summonedIndex)
    {
        return currentIndex >= 0 && summonedIndex >= 0;
    }
}