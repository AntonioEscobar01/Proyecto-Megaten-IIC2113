namespace Shin_Megami_Tensei;

public class DamageApplicationContext
{
    public CombatParticipants Participants { get; }
    public DamageContextInfo Info { get; }
    
    public IUnit Attacker => Participants.Attacker;
    public IUnit Target => Participants.Target;
    public DamageResultData DamageResult => Info.DamageResult;
    public string Affinity => Info.Affinity;
    
    public DamageApplicationContext(CombatParticipants participants, DamageContextInfo info)
    {
        Participants = participants;
        Info = info;
    }
}