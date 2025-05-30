using System.Text.Json;

namespace Shin_Megami_Tensei;

public class MonsterStatsManager
{
    private List<MonsterData> _monstersData;

    public MonsterStatsManager(string jsonFilePath = "data/monsters.json")
    {
        _monstersData = LoadMonsterData(jsonFilePath);
    }
    
    public MonsterData GetMonsterData(string name)
    {
        return _monstersData.FirstOrDefault(md => md.name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private List<MonsterData> LoadMonsterData(string jsonFilePath)
    {
        try
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            var monsterDataList = JsonSerializer.Deserialize<List<MonsterData>>(jsonContent);
            return monsterDataList ?? new List<MonsterData>();
        }
        catch (Exception ex)
        {
            return new List<MonsterData>();
        }
    }
}