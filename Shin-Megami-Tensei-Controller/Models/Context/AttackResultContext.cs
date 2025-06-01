namespace Shin_Megami_Tensei;

public class AttackResultContext
{
    public IUnit Attacker { get; }
    public IUnit Target { get; }
    public string AttackerName { get; }
    public string TargetName { get; }
    public string Affinity { get; }
    
    public AttackResultContext(IUnit attacker, IUnit target, string attackerName, string targetName, string affinity)
    {
        Attacker = attacker;
        Target = target;
        AttackerName = attackerName;
        TargetName = targetName;
        Affinity = affinity;
    }
}