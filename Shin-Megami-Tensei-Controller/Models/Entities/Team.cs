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
    
        // ✅ Obtener todos los monstruos de la reserva vivos
        for (int i = frontLineCount; i < Units.Count; i++)
        {
            var monster = Units[i];
            // ✅ Excluir placeholders y solo incluir monstruos vivos
            if (!monster.IsDead() && monster.Name != "Placeholder")
                availableMonsters.Add(monster);
        }
    
        // ✅ CORRECCIÓN: Ordenar por el orden original del archivo
        availableMonsters = availableMonsters.OrderBy(monster => 
        {
            int originalIndex = _originalMonstersOrder.IndexOf(monster.Name);
            return originalIndex == -1 ? _originalMonstersOrder.Count : originalIndex;
        }).ToList();
    
        return availableMonsters;
    }
    public void PlaceMonsterInPosition(Monster summonedMonster, int position)
    {
        // Verificar si la posición ya está ocupada por el mismo monstruo (pero muerto)
        bool isSameMosterAtPosition = position < Units.Count && Units[position] == summonedMonster;
        
        // Si el monstruo ya está en la posición deseada (incluso si está muerto), simplemente lo "revivimos"
        if (isSameMosterAtPosition)
        {
            // Si está en OrderList, lo removemos para luego añadirlo al final
            if (OrderList.Contains(summonedMonster))
                OrderList.Remove(summonedMonster);
            
            OrderList.Add(summonedMonster);
            return;
        }
        
        // Save the original index of the summoned monster (to handle reserve monsters)
        int originalSummonedIndex = Units.IndexOf(summonedMonster);
        
        // Remove the summoned monster from its current position
        Units.Remove(summonedMonster);
        
        // Modificar el criterio de posición vacía para considerar que si un monstruo está muerto,
        // se considera como una posición vacía
        bool isEmptyPosition = position >= Units.Count ||
                              (position < Units.Count && Units[position].IsDead());

        if (isEmptyPosition)
        {
            // Handle empty position - either add to end or replace a dead monster
            if (position >= Units.Count)
            {
                // Ensure we don't have gaps in the lineup
                while (Units.Count < position)
                {
                    Monster placeholder = new Monster("Placeholder");
                    placeholder.Hp = 0; // Mark as dead
                    Units.Add(placeholder);
                }
                Units.Add(summonedMonster);
            }
            else
            {
                // ✅ CORRECCIÓN: Conservar el monstruo muerto antes de reemplazarlo
                Monster deadMonster = Units[position];
                if (deadMonster.IsDead() && deadMonster.Name != "Placeholder")
                {
                    // Mover el monstruo muerto a la reserva para que pueda ser revivido
                    Units.Add(deadMonster);
                }
                Units[position] = summonedMonster;
            }
            
            if (OrderList.Contains(summonedMonster))
                OrderList.Remove(summonedMonster);
            OrderList.Add(summonedMonster);
            SortReserveMonsters();
        }
        else
        {
            // Position is occupied by another monster
            Monster replacedMonster = Units[position];
            
            // Place the summoned monster in the target position
            Units[position] = summonedMonster;
            
            // Update OrderList
            if (OrderList.Contains(replacedMonster))
            {
                int index = OrderList.IndexOf(replacedMonster);
                OrderList[index] = summonedMonster;
            }
            else if (!OrderList.Contains(summonedMonster))
            {
                OrderList.Add(summonedMonster);
            }
            
            // Always move the replaced monster to the reserve
            // First, ensure we have all frontline positions filled
            for (int i = 0; i < MAX_VISIBLE_MONSTERS; i++)
            {
                if (Units.Count <= i)
                {
                    // Add placeholders for any missing positions
                    Monster placeholder = new Monster("Placeholder");
                    placeholder.Hp = 0; // Mark as dead
                    Units.Add(placeholder);
                }
            }
            
            // Add the replaced monster to the reserve
            Units.Add(replacedMonster);
            
            // Sort the reserve monsters based on the original order
        } 
        SortReserveMonsters();
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
    private void SortReserveMonsters()
    {
        if (Units.Count <= MAX_VISIBLE_MONSTERS)
            return;
        
        var reserveMonsters = Units.Skip(MAX_VISIBLE_MONSTERS).ToList();
        
        reserveMonsters = reserveMonsters.OrderBy(monster => 
        {
            // Primero, buscar en el orden original
            int originalIndex = _originalMonstersOrder.IndexOf(monster.Name);
        
            // Si no está en el orden original, asignar un índice alto para que vaya al final
            if (originalIndex == -1)
            {
                // Para placeholders, asignar el índice más alto para que vayan al final
                if (monster.Name == "Placeholder")
                    return int.MaxValue;
            
                // Para otros monstruos no encontrados, usar el nombre para mantener consistencia
                return _originalMonstersOrder.Count + monster.Name.GetHashCode() % 1000;
            }
        
            return originalIndex;
        }).ToList();
    
        Units.RemoveRange(MAX_VISIBLE_MONSTERS, Units.Count - MAX_VISIBLE_MONSTERS);
        Units.AddRange(reserveMonsters);
    }
}