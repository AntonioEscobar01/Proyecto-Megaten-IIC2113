namespace Shin_Megami_Tensei;

public class SkillExecutionContext
{
    public CombatParticipants Participants { get; }
    public SkillExecutionInfo Info { get; }
    public int NumberOfHits { get; }
    
    public IUnit Attacker => Participants.Attacker;
    public IUnit Target => Participants.Target;
    public SkillData Skill => Info.Skill;
    public string Affinity => Info.Affinity;
    public double BaseDamage => Info.BaseDamage;
    
    public SkillExecutionContext(CombatParticipants participants, SkillExecutionInfo info, int numberOfHits)
    {
        Participants = participants;
        Info = info;
        NumberOfHits = numberOfHits;
    }
}