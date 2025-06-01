namespace Shin_Megami_Tensei;

public class DamageApplicationContext
{
    public object Attacker { get; }
    public object Target { get; }
    public DamageResultData DamageResult { get; }
    public string Affinity { get; }
    
    public DamageApplicationContext(object attacker, object target, DamageResultData damageResult, string affinity)
    {
        Attacker = attacker;
        Target = target;
        DamageResult = damageResult;
        Affinity = affinity;
    }
}