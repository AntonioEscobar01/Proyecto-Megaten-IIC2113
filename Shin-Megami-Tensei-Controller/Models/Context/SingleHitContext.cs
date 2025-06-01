namespace Shin_Megami_Tensei;

public class SingleHitContext
{
    public IUnit Attacker { get; }
    public IUnit Target { get; }
    public SkillData Skill { get; }
    public string Affinity { get; }
    public double BaseDamage { get; }
    
    public SingleHitContext(IUnit attacker, IUnit target, SkillData skill, string affinity, double baseDamage)
    {
        Attacker = attacker;
        Target = target;
        Skill = skill;
        Affinity = affinity;
        BaseDamage = baseDamage;
    }
}