namespace Shin_Megami_Tensei;

public class Team
{
    private const int MAX_SAMURAI_ALLOWED = 1;
    private const int MAX_ABILITIES_ALLOWED = 8;
    private const int MAX_VISIBLE_MONSTERS = 3;
    
    public string Player { get; private set; }
    public List<Monster> Units { get; private set; }
    public Samurai? Samurai { get; private set; }
    public List<object> OrderList { get; private set; }
    public int FullTurns { get; private set; }
    public int BlinkingTurns { get; private set; }
    public int MaxFullTurns { get; set; }  

    public Team(string player)
    {
        Player = player;
        Units = new List<Monster>();
        OrderList = new List<object>();
        FullTurns = 0;
        BlinkingTurns = 0;
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
                .OrderByDescending(unit => unit is Monster m ? m.Spd : 
                    unit is Samurai s ? s.Spd : 0)
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
        if (HasTooManyUnits(possibleUnits)) return false;
        
        HashSet<string> uniqueUnits = new HashSet<string>();
        int samuraiCount = 0;
        
        foreach (var unit in possibleUnits)
        {
            if (IsDuplicateUnit(uniqueUnits, unit)) return false;
        
            if (unit.StartsWith("[Samurai]"))
            {
                if (ExceedsSamuraiLimit(++samuraiCount)) return false;
                if (!TryAddSamuraiToTeam(unit)) return false;
            }
            else 
            {
                AddMonsterToTeam(unit);
            }
        }
        
        return HasCorrectSamuraiCount(samuraiCount);
    }
    
    private bool HasTooManyUnits(List<string> units)
    {
        return units.Count > MAX_ABILITIES_ALLOWED;
    }
    
    private bool IsDuplicateUnit(HashSet<string> uniqueUnits, string unit)
    {
        return !uniqueUnits.Add(unit);
    }
    
    private bool ExceedsSamuraiLimit(int samuraiCount)
    {
        return samuraiCount > MAX_SAMURAI_ALLOWED;
    }
    
    private bool HasCorrectSamuraiCount(int samuraiCount)
    {
        return samuraiCount == MAX_SAMURAI_ALLOWED;
    }

    private bool TryAddSamuraiToTeam(string unit)
    {
        List<string> abilities = ExtractAbilities(unit);
    
        if (HasTooManyAbilities(abilities) || HasDuplicateAbilities(abilities))
            return false;
            
        var samuraiName = unit.Replace("[Samurai]", "").Split('(')[0].Trim();
        Samurai = new Samurai(samuraiName, abilities);
        return true;
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
        MaxFullTurns = 0;
        foreach (var entity in OrderList)
        {
            if ((entity is Samurai samurai && !samurai.IsDead()) || 
                (entity is Monster monster && !monster.IsDead()))
            {
                MaxFullTurns++;
            }
        }
    }
    
    public void ResetTurns()
    {
        FullTurns = 0;
    }
    
    public bool HasCompletedAllTurns()
    {
        return FullTurns == MaxFullTurns;
    }
    
    public void CompleteTurn()
    {
        if (FullTurns >= 0)
        {
            FullTurns += 1;
        }
    }
    
    public bool AreAllUnitsDead()
    {
        return !OrderList.Any(unit => 
            (unit is Monster monster && !monster.IsDead()) || 
            (unit is Samurai samurai && !samurai.IsDead())
        );
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
}