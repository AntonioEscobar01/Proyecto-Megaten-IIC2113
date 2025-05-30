using System.Text.Json;

namespace Shin_Megami_Tensei;

public class SamuraiStatsManager
{
    private List<Stats> _samuraiStats;
    private string _jsonFilePath;

    public SamuraiStatsManager(string jsonFilePath = "data/samurai.json")
    {
        _jsonFilePath = jsonFilePath;
        _samuraiStats = LoadSamuraiStats(jsonFilePath);
    }

    public List<Stats> GetAllStats() => _samuraiStats;

    public Stats GetStatsByName(string statName)
    {
        return _samuraiStats.FirstOrDefault(s => s.Name.Equals(statName, StringComparison.OrdinalIgnoreCase));
    }

    public SamuraiData GetSamuraiData(string name)
    {
        try
        {
            string jsonContent = File.ReadAllText(_jsonFilePath);
            var samuraiDataList = JsonSerializer.Deserialize<List<SamuraiData>>(jsonContent);

            return samuraiDataList?.FirstOrDefault(s => s.name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception)
        {
            return null;
        }
    }

    private List<Stats> LoadSamuraiStats(string jsonFilePath)
    {
        try
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            var samuraiDataList = JsonSerializer.Deserialize<List<SamuraiData>>(jsonContent);

            if (samuraiDataList == null)
                return new List<Stats>();
            return samuraiDataList.Select(samuraiData => new Stats(
                samuraiData.name, samuraiData.stats.HP, samuraiData.stats.MP, samuraiData.stats.Str,
                samuraiData.stats.Skl, samuraiData.stats.Mag, samuraiData.stats.Spd, samuraiData.stats.Lck
            )).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading samurai stats: {ex.Message}");
            return new List<Stats>();
        }
    }
}