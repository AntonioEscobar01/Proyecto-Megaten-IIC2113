namespace Shin_Megami_Tensei;

public class MonsterData
{
    public string name { get; set; }
    public StatData stats { get; set; }
    public Dictionary<string, string> affinity { get; set; }
    public List<string> skills { get; set; }
}
