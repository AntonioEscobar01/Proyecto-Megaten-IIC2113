namespace Shin_Megami_Tensei;

public class MonsterData
{
    public string name { get; set; }
    public StatData stats { get; set; }
    public Dictionary<string, string> affinity { get; set; }
    public List<string> skills { get; set; }
}

public class StatData
{
    public int HP { get; set; }
    public int MP { get; set; }
    public int Str { get; set; }
    public int Skl { get; set; }
    public int Mag { get; set; }
    public int Spd { get; set; }
    public int Lck { get; set; }
}