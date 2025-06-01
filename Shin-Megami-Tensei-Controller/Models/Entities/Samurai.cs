namespace Shin_Megami_Tensei;

public class Samurai : UnitBase
{
    private static SamuraiStatsManager _statsManager;

    static Samurai()
    {
        _statsManager = new SamuraiStatsManager();
    }

    public Samurai(string name, List<string> abilities) : base(name)
    {
        SetAbilities(abilities);
        LoadStats();
    }

    protected override void LoadStats()
    {
        var stats = _statsManager.GetStatsByName(GetName());
        SetOriginalHp(stats.Hp);
        SetOriginalMp(stats.Mp);
        SetOriginalStr(stats.Str);
        SetOriginalSkl(stats.Skl);
        SetOriginalMag(stats.Mag);
        SetOriginalSpd(stats.Spd);
        SetOriginalLck(stats.Lck);
        CopyOriginalStatsToCurrent();
        
        LoadAffinities();
    }
    
    private void LoadAffinities()
    {
        var samuraiData = _statsManager.GetSamuraiData(GetName());
    
        if (HasValidAffinityData(samuraiData))
        {
            SetAffinities(new Affinity(samuraiData.affinity));
        }
    }

    private bool HasValidAffinityData(SamuraiData samuraiData)
    {
        return samuraiData != null && samuraiData.affinity != null;
    }
}