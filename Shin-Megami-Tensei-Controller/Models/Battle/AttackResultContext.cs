namespace Shin_Megami_Tensei;

public class AttackResultContext
{
    public object Attacker { get; }
    public object Target { get; }
    public string AttackerName { get; }
    public string TargetName { get; }
    public string Affinity { get; }
    
    public AttackResultContext(object attacker, object target, string attackerName, string targetName, string affinity)
    {
        Attacker = attacker;
        Target = target;
        AttackerName = attackerName;
        TargetName = targetName;
        Affinity = affinity;
    }
}