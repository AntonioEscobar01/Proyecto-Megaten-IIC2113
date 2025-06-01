namespace Shin_Megami_Tensei;

public class DamageApplicationContext
{
    public IUnit Attacker { get; }
    public IUnit Target { get; }
    public DamageResultData DamageResult { get; }
    public string Affinity { get; }
    
    public DamageApplicationContext(IUnit attacker, IUnit target, DamageResultData damageResult, string affinity)
    {
        Attacker = attacker;
        Target = target;
        DamageResult = damageResult;
        Affinity = affinity;
    }
}