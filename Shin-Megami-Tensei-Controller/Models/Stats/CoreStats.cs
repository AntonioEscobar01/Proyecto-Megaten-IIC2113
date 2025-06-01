namespace Shin_Megami_Tensei;

public class CoreStats
{
    public int Hp { get; }
    public int Mp { get; }
    
    public CoreStats(int hp, int mp)
    {
        Hp = hp;
        Mp = mp;
    }
}