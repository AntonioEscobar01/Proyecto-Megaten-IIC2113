namespace Shin_Megami_Tensei;

public class MonsterData
{
    public required string name { get; set; }
    public required StatData stats { get; set; }
    public required Dictionary<string, string> affinity { get; set; }
    public required List<string> skills { get; set; }
}