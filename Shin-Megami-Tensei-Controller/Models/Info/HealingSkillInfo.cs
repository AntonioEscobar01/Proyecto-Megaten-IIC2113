namespace Shin_Megami_Tensei;

public class HealingSkillInfo
{
    public SkillData SkillData { get; }
    public HealingSkillClassification Classification { get; }
    
    public HealingSkillInfo(SkillData skillData, HealingSkillClassification classification)
    {
        SkillData = skillData;
        Classification = classification;
    }
}