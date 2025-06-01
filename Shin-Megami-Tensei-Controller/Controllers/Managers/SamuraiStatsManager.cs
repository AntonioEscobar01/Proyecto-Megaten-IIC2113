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
    

    public Stats? GetStatsByName(string statName)
    {
        return _samuraiStats.FirstOrDefault(s => IsStatNameMatch(s, statName));
    }

    public SamuraiData? GetSamuraiData(string name)
    {
        try
        {
            string jsonContent = File.ReadAllText(_jsonFilePath);
            var samuraiDataList = JsonSerializer.Deserialize<List<SamuraiData>>(jsonContent);

            return samuraiDataList?.FirstOrDefault(s => IsSamuraiNameMatch(s, name));
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

            if (IsSamuraiDataNull(samuraiDataList))
                return new List<Stats>();
                
            return ConvertToStatsList(samuraiDataList);
        }
        catch (Exception)
        {
            return new List<Stats>();
        }
    }

    private bool IsStatNameMatch(Stats stat, string statName)
    {
        return stat.Name.Equals(statName, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsSamuraiNameMatch(SamuraiData samuraiData, string name)
    {
        return samuraiData.name.Equals(name, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsSamuraiDataNull(List<SamuraiData>? samuraiDataList)
    {
        return samuraiDataList == null;
    }

    private List<Stats> ConvertToStatsList(List<SamuraiData> samuraiDataList)
    {
        return samuraiDataList.Select(samuraiData => 
        {
            var coreStats = new CoreStats(samuraiData.stats.HP, samuraiData.stats.MP);
            var battleStats = new BattleStats(samuraiData.stats.Str, samuraiData.stats.Skl, samuraiData.stats.Mag);
            var secondaryStats = new SecondaryStats(samuraiData.stats.Spd, samuraiData.stats.Lck);
            var completeStats = new CompleteStats(coreStats, battleStats, secondaryStats);
            return new Stats(samuraiData.name, completeStats);
        }).ToList();
    }
}