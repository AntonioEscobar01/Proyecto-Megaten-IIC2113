namespace Shin_Megami_Tensei;

public class SamuraiData
{
    public required string name { get; set; }
    public required StatData stats { get; set; }
    public required Dictionary<string, string> affinity { get; set; }
}