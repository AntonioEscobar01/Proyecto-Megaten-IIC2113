namespace Shin_Megami_Tensei;

public class AttackResultContext
{
    public CombatParticipants Participants { get; }
    public string Affinity { get; }
    
    public IUnit Attacker => Participants.Attacker;
    public IUnit Target => Participants.Target;
    public string AttackerName => Participants.Attacker.GetName();
    public string TargetName => Participants.Target.GetName();
    
    public AttackResultContext(CombatParticipants participants, string affinity)
    {
        Participants = participants;
        Affinity = affinity;
    }
}