namespace Shin_Megami_Tensei;

public class SkillExecutionContext
{
    public object Attacker { get; }
    public object Target { get; }
    public SkillData Skill { get; }
    public string Affinity { get; }
    public double BaseDamage { get; }
    public int NumberOfHits { get; }
    
    public SkillExecutionContext(object attacker, object target, SkillData skill, string affinity, double baseDamage, int numberOfHits)
    {
        Attacker = attacker;
        Target = target;
        Skill = skill;
        Affinity = affinity;
        BaseDamage = baseDamage;
        NumberOfHits = numberOfHits;
    }
}