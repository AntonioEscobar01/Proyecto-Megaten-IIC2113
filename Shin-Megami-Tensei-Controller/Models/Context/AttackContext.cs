namespace Shin_Megami_Tensei;

public class AttackContext
{
    public CombatParticipants Participants { get; }
    public AttackInfo Info { get; }
    
    public IUnit Attacker => Participants.Attacker;
    public IUnit Target => Participants.Target;
    public double BaseDamage => Info.BaseDamage;
    public string AttackType => Info.AttackType;
    public string ActionType => Info.ActionType;
    
    public AttackContext(CombatParticipants participants, AttackInfo info)
    {
        Participants = participants;
        Info = info;
    }
}