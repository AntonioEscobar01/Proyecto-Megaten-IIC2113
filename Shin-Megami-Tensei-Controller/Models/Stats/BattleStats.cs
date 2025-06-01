namespace Shin_Megami_Tensei;

public class BattleStats
{
    public int Str { get; }
    public int Skl { get; }
    public int Mag { get; }
    
    public BattleStats(int str, int skl, int mag)
    {
        Str = str;
        Skl = skl;
        Mag = mag;
    }
}