namespace Shin_Megami_Tensei;

public class SkillExecutionInfo
{
    public SkillData Skill { get; }
    public string Affinity { get; }
    public double BaseDamage { get; }
    
    public SkillExecutionInfo(SkillData skill, string affinity, double baseDamage)
    {
        Skill = skill;
        Affinity = affinity;
        BaseDamage = baseDamage;
    }
}