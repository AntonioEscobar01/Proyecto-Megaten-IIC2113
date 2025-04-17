using System.Text.Json;

namespace Shin_Megami_Tensei;

public class SamuraiStatsManager
{
    private List<Stats> _samuraiStats;

    public SamuraiStatsManager(string jsonFilePath = "data/samurai.json")
    {
        _samuraiStats = LoadSamuraiStats(jsonFilePath);
    }

    public List<Stats> GetAllStats() => _samuraiStats;

    public Stats GetStatsByName(string statName)
    {
        return _samuraiStats.FirstOrDefault(s => s.Name.Equals(statName, StringComparison.OrdinalIgnoreCase));
    }

    private List<Stats> LoadSamuraiStats(string jsonFilePath)
    {
        try
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            var samuraiDataList = JsonSerializer.Deserialize<List<SamuraiData>>(jsonContent);

            if (samuraiDataList == null)
                return new List<Stats>();
            return samuraiDataList.Select(sd => new Stats(
                sd.name, sd.stats.HP, sd.stats.MP, sd.stats.Str,
                sd.stats.Skl, sd.stats.Mag, sd.stats.Spd, sd.stats.Lck
            )).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading samurai stats: {ex.Message}");
            return new List<Stats>();
        }
    }
}