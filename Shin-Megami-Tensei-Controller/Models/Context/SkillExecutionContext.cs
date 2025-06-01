namespace Shin_Megami_Tensei;

public class SkillExecutionContext
{
    public IUnit Attacker { get; }
    public IUnit Target { get; }
    public SkillData Skill { get; }
    public string Affinity { get; }
    public double BaseDamage { get; }
    public int NumberOfHits { get; }
    
    public SkillExecutionContext(IUnit attacker, IUnit target, SkillData skill, string affinity, double baseDamage, int numberOfHits)
    {
        Attacker = attacker;
        Target = target;
        Skill = skill;
        Affinity = affinity;
        BaseDamage = baseDamage;
        NumberOfHits = numberOfHits;
    }
}