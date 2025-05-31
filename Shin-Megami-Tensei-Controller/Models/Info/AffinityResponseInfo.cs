namespace Shin_Megami_Tensei;

public class AffinityResponseInfo
{
    public string AttackerName { get; }
    public string Affinity { get; }
    public string TargetName { get; }
    public int Damage { get; }
    
    public AffinityResponseInfo(string attackerName, string affinity, string targetName, int damage)
    {
        AttackerName = attackerName;
        Affinity = affinity;
        TargetName = targetName;
        Damage = damage;
    }
}