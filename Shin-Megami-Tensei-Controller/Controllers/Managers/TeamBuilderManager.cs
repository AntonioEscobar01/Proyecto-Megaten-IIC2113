namespace Shin_Megami_Tensei;

public class TeamBuilderManager
{
    public (Samurai? samurai, List<string> monsterNames) BuildTeamFromUnits(List<string> possibleUnits)
    {
        Samurai? samurai = null;
        var monsterNames = new List<string>();

        foreach (var unit in possibleUnits)
        {
            ProcessUnitCreation(unit, ref samurai, monsterNames);
        }

        return (samurai, monsterNames);
    }

    public List<string> ExtractMonsterNames(List<string> possibleUnits)
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

    private void ProcessUnitCreation(string unit, ref Samurai? samurai, List<string> monsterNames)
    {
        if (IsSamuraiUnit(unit))
        {
            samurai = CreateSamurai(unit);
        }
        else 
        {
            monsterNames.Add(unit);
        }
    }

    private Samurai CreateSamurai(string unit)
    {
        List<string> abilities = ExtractAbilities(unit);
        string samuraiName = ExtractSamuraiName(unit);
        return new Samurai(samuraiName, abilities);
    }

    private string ExtractSamuraiName(string unit)
    {
        string withoutPrefix = unit.Replace("[Samurai]", "");
        string[] parts = withoutPrefix.Split('(');
        return parts[0].Trim();
    }

    private bool IsSamuraiUnit(string unit)
    {
        return unit.StartsWith("[Samurai]");
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
}