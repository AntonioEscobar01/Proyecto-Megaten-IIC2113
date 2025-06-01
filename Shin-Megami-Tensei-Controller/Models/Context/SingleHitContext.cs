namespace Shin_Megami_Tensei;

public class SingleHitContext
{
    public CombatParticipants Participants { get; }
    public SkillExecutionInfo Info { get; }
    
    public IUnit Attacker => Participants.Attacker;
    public IUnit Target => Participants.Target;
    public SkillData Skill => Info.Skill;
    public string Affinity => Info.Affinity;
    public double BaseDamage => Info.BaseDamage;
    
    public SingleHitContext(CombatParticipants participants, SkillExecutionInfo info)
    {
        Participants = participants;
        Info = info;
    }
}