using System.Text.Json;

namespace Shin_Megami_Tensei;

public class SkillsManager
{
    private List<SkillData> _skills;

    public SkillsManager(string jsonFilePath = "data/skills.json")
    {
        _skills = LoadSkills(jsonFilePath);
    }

    public List<SkillData> GetAllSkills() => _skills;

    public SkillData? GetSkillByName(string name)
    {
        return _skills.FirstOrDefault(s => IsSkillNameMatch(s, name));
    }
    
    private bool IsSkillNameMatch(SkillData skill, string name)
    {
        return skill.name.Equals(name, StringComparison.OrdinalIgnoreCase);
    }

    public int GetSkillCost(string name)
    {
        var skill = GetSkillByName(name);
        return skill?.cost ?? -1;
    }
    

    private List<SkillData> LoadSkills(string jsonFilePath)
    {
        try
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            var skillsList = JsonSerializer.Deserialize<List<SkillData>>(jsonContent);
            
            return skillsList ?? new List<SkillData>();
        }
        catch (Exception)
        {
            return new List<SkillData>();
        }
    }
}