namespace Shin_Megami_Tensei;

public class AffinityResponseData
{
    public string Affinity { get; }
    public int Damage { get; }
    
    public AffinityResponseData(string affinity, int damage)
    {
        Affinity = affinity;
        Damage = damage;
    }
}