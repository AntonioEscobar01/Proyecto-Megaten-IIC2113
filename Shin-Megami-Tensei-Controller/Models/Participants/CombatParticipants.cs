namespace Shin_Megami_Tensei;

public class CombatParticipants
{
    public IUnit Attacker { get; }
    public IUnit Target { get; }
    
    public CombatParticipants(IUnit attacker, IUnit target)
    {
        Attacker = attacker;
        Target = target;
    }
}