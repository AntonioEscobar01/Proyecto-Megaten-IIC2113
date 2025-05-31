namespace Shin_Megami_Tensei;

public class AttackContext
{
    public object Attacker { get; }
    public object Target { get; }
    public double BaseDamage { get; }
    public string AttackType { get; }
    public string ActionType { get; }
    
    public AttackContext(object attacker, object target, double baseDamage, string attackType, string actionType)
    {
        Attacker = attacker;
        Target = target;
        BaseDamage = baseDamage;
        AttackType = attackType;
        ActionType = actionType;
    }
}