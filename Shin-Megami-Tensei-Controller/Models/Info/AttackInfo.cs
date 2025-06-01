namespace Shin_Megami_Tensei;

public class AttackInfo
{
    public double BaseDamage { get; }
    public string AttackType { get; }
    public string ActionType { get; }
    
    public AttackInfo(double baseDamage, string attackType, string actionType)
    {
        BaseDamage = baseDamage;
        AttackType = attackType;
        ActionType = actionType;
    }
}