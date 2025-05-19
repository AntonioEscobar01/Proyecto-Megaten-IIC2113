// ...existing code...

public void SortOrderListBySpeed()
{
    List<UnitBase> orderedList = new List<UnitBase>();
    foreach (Samurai samurai in Samurais)
    {
        orderedList.Add(samurai);
    }
    foreach (Monster monster in Monsters)
    {
        orderedList.Add(monster);
    }
    OrderList = orderedList.OrderByDescending(unit => unit.Speed).ToList();
}

public bool HasExceededSamuraiLimit()
{
    return Samurais.Count >= SamuraisLimit;
}

public bool CanAddSamuraiToTeam(string samuraiName)
{
    if (HasExceededSamuraiLimit())
    {
        return false;
    }
    
    CreateAndAddSamurai(samuraiName);
    return true;
}

private void CreateAndAddSamurai(string samuraiName)
{
    Samurai samurai = new Samurai
    {
        Name = samuraiName
    };
    Samurais.Add(samurai);
}

public bool ValidateTeam()
{
    if (Samurais.Count == 0)
    {
        return false;
    }
    
    AddDefaultMonstersIfNeeded();
    return true;
}

private void AddDefaultMonstersIfNeeded()
{
    if (Monsters.Count == 0)
    {
        for (int i = 0; i < MonstersLimit; i++)
        {
            Monster monster = new Monster
            {
                Name = "empty"
            };
            Monsters.Add(monster);
        }
    }
}

public bool PlaceMonsterInPosition(string monsterName, int position)
{
    if (!IsValidMonsterPosition(position))
    {
        return false;
    }
    
    return AddOrReplaceMonster(monsterName, position);
}

private bool IsValidMonsterPosition(int position)
{
    return position >= 0 && position < MonstersLimit;
}

private bool AddOrReplaceMonster(string monsterName, int position)
{
    if (string.IsNullOrEmpty(monsterName) || monsterName == "empty")
    {
        RemoveMonsterAtPosition(position);
        return true;
    }
    
    return CreateAndAddMonsterAtPosition(monsterName, position);
}

private void RemoveMonsterAtPosition(int position)
{
    if (position < Monsters.Count)
    {
        Monsters[position] = new Monster { Name = "empty" };
    }
}

private bool CreateAndAddMonsterAtPosition(string monsterName, int position)
{
    Monster monster = CreateMonster(monsterName);
    if (monster == null)
    {
        return false;
    }
    
    if (position < Monsters.Count)
    {
        Monsters[position] = monster;
    }
    else
    {
        Monsters.Add(monster);
    }
    
    return true;
}

// Refactorizando el train wreck
public List<string> ExtractAbilities(string unit)
{
    string trimmedLine = unit.Split('|')[4].TrimEnd();
    string[] abilitiesArray = trimmedLine.Split(',');
    return abilitiesArray.Select(ability => ability.Trim()).ToList();
}

// ...existing code...
