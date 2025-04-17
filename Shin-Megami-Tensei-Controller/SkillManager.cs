using System.Text.Json;

namespace Shin_Megami_Tensei;

public class SkillData
{
    public string name { get; set; }
    public string type { get; set; }
    public int cost { get; set; }
    public int power { get; set; }
    public string target { get; set; }
    public string hits { get; set; }
    public string effect { get; set; }
}

public class SkillsManager
{
    private List<SkillData> _skills;

    public SkillsManager(string jsonFilePath = "data/skills.json")
    {
        _skills = LoadSkills(jsonFilePath);
    }

    public List<SkillData> GetAllSkills() => _skills;

    public SkillData GetSkillByName(string name)
    {
        return _skills.FirstOrDefault(s => s.name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public int GetSkillCost(string name)
    {
        var skill = GetSkillByName(name);
        return skill?.cost ?? -1;
    }

    public string GetSkillEffect(string name)
    {
        var skill = GetSkillByName(name);
        return skill?.effect ?? "Skill not found";
    }

    private List<SkillData> LoadSkills(string jsonFilePath)
    {
        try
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            var skillsList = JsonSerializer.Deserialize<List<SkillData>>(jsonContent);
            
            return skillsList ?? new List<SkillData>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading skills: {ex.Message}");
            return new List<SkillData>();
        }
    }
}