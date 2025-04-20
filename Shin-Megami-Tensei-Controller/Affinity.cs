namespace Shin_Megami_Tensei;

public class Affinity
{
    private readonly Dictionary<string, string> _affinities;

    public Affinity(Dictionary<string, string> affinities)
    {
        _affinities = affinities ?? new Dictionary<string, string>();
    }

    public string GetAffinity(string type)
    {
        return _affinities.TryGetValue(type, out var value) ? value : "-";
    }

    public Dictionary<string, string> GetAllAffinities()
    {
        return new Dictionary<string, string>(_affinities);
    }
}