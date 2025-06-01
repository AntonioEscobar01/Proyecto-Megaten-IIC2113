namespace Shin_Megami_Tensei;

public class DamageContextInfo
{
    public DamageResultData DamageResult { get; }
    public string Affinity { get; }
    
    public DamageContextInfo(DamageResultData damageResult, string affinity)
    {
        DamageResult = damageResult;
        Affinity = affinity;
    }
}