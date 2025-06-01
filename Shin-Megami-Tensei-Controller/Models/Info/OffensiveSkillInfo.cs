namespace Shin_Megami_Tensei;

public class OffensiveSkillInfo
{
    public SkillData Skill { get; }
    public int UsedSkillsCount { get; }
    
    public OffensiveSkillInfo(SkillData skill, int usedSkillsCount)
    {
        Skill = skill;
        UsedSkillsCount = usedSkillsCount;
    }
}