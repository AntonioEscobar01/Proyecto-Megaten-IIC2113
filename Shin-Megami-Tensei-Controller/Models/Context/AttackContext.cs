namespace Shin_Megami_Tensei;

public class AttackContext
{
    public IUnit Attacker { get; }
    public IUnit Target { get; }
    public double BaseDamage { get; }
    public string AttackType { get; }
    public string ActionType { get; }
    
    public AttackContext(IUnit attacker, IUnit target, double baseDamage, string attackType, string actionType)
    {
        Attacker = attacker;
        Target = target;
        BaseDamage = baseDamage;
        AttackType = attackType;
        ActionType = actionType;
    }
}