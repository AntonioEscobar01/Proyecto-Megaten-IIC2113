
namespace Shin_Megami_Tensei;

public class Team
{
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
        if (Samurai != null)
            OrderList.Add(Samurai);
        for (int i = 0; i < Math.Min(Units.Count, 3); i++)
            OrderList.Add(Units[i]);
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
        if (possibleUnits.Count > 8) return false;
        HashSet<string> uniqueUnits = new HashSet<string>();
        int samuraiCount = 0;
        foreach (var unit in possibleUnits)
        {
            if (!uniqueUnits.Add(unit)) return false;
        
            if (unit.StartsWith("[Samurai]"))
            {
                if (++samuraiCount > 1) return false;
                if (!ProcessSamuraiUnit(unit)) return false;
            }
            else ProcessMonsterUnit(unit);
        }
        return samuraiCount == 1;
    }

    private bool ProcessSamuraiUnit(string unit)
    {
        List<string> abilities = ExtractAbilities(unit);
    
        if (abilities.Count > 8 || abilities.Distinct().Count() != abilities.Count)
            return false;
        var samuraiName = unit.Replace("[Samurai]", "").Split('(')[0].Trim();
        Samurai = new Samurai(samuraiName, abilities);
        return true;
    }

    private List<string> ExtractAbilities(string unit)
    {
        if (unit.Contains("("))
            return unit.Split('(')[1].TrimEnd(')').Split(',').Select(h => h.Trim()).ToList();
    
        return new List<string>();
    }

    private void ProcessMonsterUnit(string unit)
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
    
    public void TurnComplete()
    {
        if (FullTurns >= 0)
        {
            FullTurns += 1;
        }
    }
    
    public bool AllUnitsDead()
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