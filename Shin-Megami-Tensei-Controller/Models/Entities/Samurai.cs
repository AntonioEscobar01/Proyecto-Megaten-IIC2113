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
        Abilities = abilities;
        LoadStats();
    }

    protected override void LoadStats()
    {
        var stats = _statsManager.GetStatsByName(Name);
        OriginalHp = stats.Hp;
        OriginalMp = stats.Mp;
        OriginalStr = stats.Str;
        OriginalSkl = stats.Skl;
        OriginalMag = stats.Mag;
        OriginalSpd = stats.Spd;
        OriginalLck = stats.Lck;
        CopyOriginalStatsToCurrent();
        
        LoadAffinities();
    }
    
    private void LoadAffinities()
    {
        var samuraiData = _statsManager.GetSamuraiData(Name);
    
        if (HasValidAffinityData(samuraiData))
        {
            Affinities = new Affinity(samuraiData.affinity);
        }
    }

    private bool HasValidAffinityData(SamuraiData samuraiData)
    {
        return samuraiData != null && samuraiData.affinity != null;
    }

}