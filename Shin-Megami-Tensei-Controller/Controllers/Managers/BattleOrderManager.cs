namespace Shin_Megami_Tensei;

public class BattleOrderManager
{
    private const int MAX_VISIBLE_MONSTERS = 3;
    private List<IUnit> _orderList;

    public BattleOrderManager()
    {
        _orderList = new List<IUnit>();
    }

    public List<IUnit> GetOrderList() => _orderList;

    public IUnit? GetCurrentUnit()
    {
        return _orderList.Count > 0 ? _orderList[0] : null;
    }

    public void InitializeOrderList(Samurai? samurai, List<Monster> units)
    {
        _orderList.Clear();
        AddUnitsToOrderList(samurai, units);
        SortOrderListBySpeed();
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

    public void RemoveDeadUnits()
    {
        _orderList.RemoveAll(unit => unit.IsDead());
    }

    public bool AreAllUnitsDead()
    {
        return !_orderList.Any(unit => !unit.IsDead());
    }

    public void AddUnitToOrderList(IUnit unit)
    {
        if (!_orderList.Contains(unit))
            _orderList.Add(unit);
    }

    public void RemoveUnitFromOrderList(IUnit unit)
    {
        if (_orderList.Contains(unit))
            _orderList.Remove(unit);
    }

    public bool IsUnitInOrderList(IUnit unit)
    {
        return _orderList.Contains(unit);
    }

    public void ReplaceUnitInOrderList(IUnit oldUnit, IUnit newUnit)
    {
        int index = _orderList.IndexOf(oldUnit);
        if (index >= 0)
            _orderList[index] = newUnit;
    }

    private void AddUnitsToOrderList(Samurai? samurai, List<Monster> units)
    {
        if (samurai != null)
            _orderList.Add(samurai);
        
        for (int i = 0; i < Math.Min(units.Count, MAX_VISIBLE_MONSTERS); i++)
            _orderList.Add(units[i]);
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

    private bool HasUnitsInOrder()
    {
        return _orderList.Count > 0;
    }
}