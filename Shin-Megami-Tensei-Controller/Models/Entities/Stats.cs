namespace Shin_Megami_Tensei;


public class Stats
{
    public string Name;
    public int Hp;
    public int Mp;
    public int Str;
    public int Skl;
    public int Mag;
    public int Spd;
    public int Lck;
    
    public Stats(string name, int hp, int mp, int str, int skl, int mag, int spd, int lck)
    {
        Name = name;
        Hp = hp;
        Mp = mp;
        Str = str;
        Skl = skl;
        Mag = mag;
        Spd = spd;
        Lck = lck;
    }
}