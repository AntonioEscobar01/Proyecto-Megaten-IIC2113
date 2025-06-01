namespace Shin_Megami_Tensei;

public class TeamValidatorManager
{
    private const int MAX_SAMURAI_ALLOWED = 1;
    private const int MAX_ABILITIES_ALLOWED = 8;

    public bool IsValidTeam(List<string> possibleUnits)
    {
        return IsTeamStructureValid(possibleUnits);
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