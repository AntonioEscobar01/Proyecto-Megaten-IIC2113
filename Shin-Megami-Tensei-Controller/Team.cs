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
    private List<string> _originalMonstersOrder;
    public List<object> OrderList { get; private set; }
    public int FullTurns { get; private set; }
    public int BlinkingTurns { get; private set; }
    public int MaxFullTurns { get; private set; } 
    public int UsedSkillsCount { get; private set; }

    public Team(string player)
    {
        Player = player;
        Units = new List<Monster>();
        OrderList = new List<object>();
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
        _originalMonstersOrder = possibleUnits
            .Where(name => !name.Contains("[Samurai]"))
            .ToList();
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
        int frontLineCount = Math.Min(Units.Count, 3);

        for (int i = frontLineCount; i < Units.Count; i++)
        {
            if (!Units[i].IsDead())
                availableMonsters.Add(Units[i]);
        }
        return availableMonsters;
    }

    public void PlaceMonsterInPosition(Monster summonedMonster, int position)
    {
        // Guardar el índice original del monstruo invocado
        int originalSummonedIndex = Units.IndexOf(summonedMonster);

        // Eliminar el monstruo de su posición actual
        Units.Remove(summonedMonster);

        // Verificar si la posición está vacía (fuera de rango o monstruo muerto)
        bool isEmptyPosition = position >= Units.Count ||
                              (position < Units.Count && Units[position].IsDead());

        if (isEmptyPosition)
        {
            // Colocar el monstruo en la posición especificada
            if (position >= Units.Count)
                Units.Add(summonedMonster);
            else
                Units[position] = summonedMonster;

            // Asegurar que el monstruo esté al final de la lista de orden
            if (OrderList.Contains(summonedMonster))
                OrderList.Remove(summonedMonster);
                
            // Añadir el monstruo al final de la lista de orden
            OrderList.Add(summonedMonster);

            // Eliminar esta línea para evitar la rotación duplicada
            // RotateOrderList();
            
            // Ordenar las unidades de reserva para mostrarlas correctamente
            SortReserveMonsters();
        }
        else
        {
            // Si reemplazamos un monstruo existente, mantener el comportamiento actual
            Monster replacedMonster = Units[position];
            Units[position] = summonedMonster;

            // Actualizar la posición en OrderList si el reemplazado estaba allí
            if (OrderList.Contains(replacedMonster))
            {
                int index = OrderList.IndexOf(replacedMonster);
                OrderList[index] = summonedMonster;
            }
            else if (!OrderList.Contains(summonedMonster))
            {
                OrderList.Add(summonedMonster);
            }

            // Colocar el monstruo reemplazado en su nueva posición
            if (originalSummonedIndex >= 0 && originalSummonedIndex < Units.Count)
            {
                Units.Insert(originalSummonedIndex, replacedMonster);
            }
            else
            {
                Units.Add(replacedMonster);
            }

            // Ordenar las unidades de reserva
            SortReserveMonsters();
        }
    }

    public void SwapMonsters(Monster currentMonster, Monster summonedMonster)
    {
        // Obtener índices
        int currentIndex = Units.IndexOf(currentMonster);
        int summonedIndex = Units.IndexOf(summonedMonster);
    
        // Intercambiar posiciones en la lista de unidades
        if (currentIndex >= 0 && summonedIndex >= 0)
        {
            Units[currentIndex] = summonedMonster;
            Units[summonedIndex] = currentMonster;
        }
    
        // Mantener la posición en la lista de orden
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
    
    private void SortReserveMonsters()
    {
        // Separar los monstruos de reserva
        var reserveMonsters = Units.Skip(MAX_VISIBLE_MONSTERS).ToList();
    
        // Ordenarlos según el orden original
        reserveMonsters = reserveMonsters.OrderBy(monster => 
            _originalMonstersOrder.IndexOf(monster.Name)).ToList();
    
        // Reemplazar la sección de reserva en la lista Units
        Units.RemoveRange(MAX_VISIBLE_MONSTERS, Units.Count - MAX_VISIBLE_MONSTERS);
        Units.AddRange(reserveMonsters);
    }
}