namespace Shin_Megami_Tensei;

public class SingleHitContext
{
    public object Attacker { get; }
    public object Target { get; }
    public SkillData Skill { get; }
    public string Affinity { get; }
    public double BaseDamage { get; }
    
    public SingleHitContext(object attacker, object target, SkillData skill, string affinity, double baseDamage)
    {
        Attacker = attacker;
        Target = target;
        Skill = skill;
        Affinity = affinity;
        BaseDamage = baseDamage;
    }
}