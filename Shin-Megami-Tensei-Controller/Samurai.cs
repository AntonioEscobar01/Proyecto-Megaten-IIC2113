namespace Shin_Megami_Tensei;

public class Samurai
{
    public string Name { get; private set; }
    public List<string> Abilities { get; private set; }
    public Affinity Affinities { get; private set; }
    
    public int OriginalHp { get; private set; }
    public int OriginalMp { get; private set; }
    public int OriginalStr { get; private set; }
    public int OriginalSkl { get; private set; }
    public int OriginalMag { get; private set; }
    public int OriginalSpd { get; private set; }
    public int OriginalLck { get; private set; }
    
    public int Hp { get; set; }
    public int Mp { get; set; }
    public int Str { get; set; }
    public int Skl { get; set; }
    public int Mag { get; set; }
    public int Spd { get; set; }
    public int Lck { get; set; }

    private static SamuraiStatsManager _statsManager;

    static Samurai()
    {
        _statsManager = new SamuraiStatsManager();
    }

    public Samurai(string name, List<string> abilities)
    {
        Name = name;
        Abilities = abilities;
        LoadStats();
        LoadAffinities();
    }

    private void LoadStats()
    {
        var stats = _statsManager.GetStatsByName(Name);
        OriginalHp = stats.Hp;
        OriginalMp = stats.Mp;
        OriginalStr = stats.Str;
        OriginalSkl = stats.Skl;
        OriginalMag = stats.Mag;
        OriginalSpd = stats.Spd;
        OriginalLck = stats.Lck;
        Hp = OriginalHp;
        Mp = OriginalMp;
        Str = OriginalStr;
        Skl = OriginalSkl;
        Mag = OriginalMag;
        Spd = OriginalSpd;
        Lck = OriginalLck;
    }
    
    private void LoadAffinities()
    {
        var samuraiData = _statsManager.GetSamuraiData(Name);
        if (samuraiData != null && samuraiData.affinity != null)
        {
            Affinities = new Affinity(samuraiData.affinity);
        }
        else
        {
            Affinities = new Affinity(new Dictionary<string, string>());
        }
    }
    
    public void ResetStats()
    {
        Hp = OriginalHp;
        Mp = OriginalMp;
        Str = OriginalStr;
        Skl = OriginalSkl;
        Mag = OriginalMag;
        Spd = OriginalSpd;
        Lck = OriginalLck;
    }
    
    public bool IsDead()
    {
        return Hp <= 0;
    }
}