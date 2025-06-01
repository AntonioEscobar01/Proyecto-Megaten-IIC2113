namespace Shin_Megami_Tensei;

public class Stats
{
    public string Name { get; }
    public CompleteStats AllStats { get; }
    
    public int Hp => AllStats.Core.Hp;
    public int Mp => AllStats.Core.Mp;
    public int Str => AllStats.Battle.Str;
    public int Skl => AllStats.Battle.Skl;
    public int Mag => AllStats.Battle.Mag;
    public int Spd => AllStats.Secondary.Spd;
    public int Lck => AllStats.Secondary.Lck;
    
    public Stats(string name, CompleteStats allStats)
    {
        Name = name;
        AllStats = allStats;
    }
}