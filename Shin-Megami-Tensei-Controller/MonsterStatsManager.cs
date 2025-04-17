using System.Text.Json;

namespace Shin_Megami_Tensei;

public class MonsterStatsManager
{
    private List<MonsterData> _monstersData;

    public MonsterStatsManager(string jsonFilePath = "data/monsters.json")
    {
        _monstersData = LoadMonsterData(jsonFilePath);
    }

    public List<Stats> GetAllStats()
    {
        return _monstersData.Select(md => new Stats(
            md.name,
            md.stats.HP,
            md.stats.MP,
            md.stats.Str,
            md.stats.Skl,
            md.stats.Mag,
            md.stats.Spd,
            md.stats.Lck
        )).ToList();
    }

    public Stats GetStatsByName(string name)
    {
        var monsterData = _monstersData.FirstOrDefault(md => md.name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (monsterData == null)
            return null;

        return new Stats(
            monsterData.name,
            monsterData.stats.HP,
            monsterData.stats.MP,
            monsterData.stats.Str,
            monsterData.stats.Skl,
            monsterData.stats.Mag,
            monsterData.stats.Spd,
            monsterData.stats.Lck
        );
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