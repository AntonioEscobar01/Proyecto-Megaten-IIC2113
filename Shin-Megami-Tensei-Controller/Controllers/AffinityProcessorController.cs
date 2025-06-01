namespace Shin_Megami_Tensei;

public class AffinityProcessorController
{
    private const string RESIST_AFFINITY = "Rs";
    private const string WEAK_AFFINITY = "Wk";
    private const string NULL_AFFINITY = "Nu";
    private const string REPEL_AFFINITY = "Rp";
    private const string DRAIN_AFFINITY = "Dr";
    
    private const double RESIST_DAMAGE_FACTOR = 0.5;
    private const double WEAK_DAMAGE_FACTOR = 1.5;

    public DamageResultData ProcessAffinityDamage(double baseDamage, string affinity)
    {
        return affinity switch
        {
            RESIST_AFFINITY => CreateResistDamageResult(baseDamage),
            WEAK_AFFINITY => CreateWeakDamageResult(baseDamage),
            NULL_AFFINITY => CreateNullDamageResult(),
            REPEL_AFFINITY => CreateRepelDamageResult(baseDamage),
            DRAIN_AFFINITY => CreateDrainDamageResult(baseDamage),
            _ => CreateNormalDamageResult(baseDamage)
        };
    }

    public bool ShouldShowAttackerResult(string affinity)
    {
        return affinity == REPEL_AFFINITY;
    }

    private DamageResultData CreateResistDamageResult(double baseDamage)
    {
        return new DamageResultData(Convert.ToInt32(Math.Floor(baseDamage * RESIST_DAMAGE_FACTOR)), DamageType.Resist);
    }

    private DamageResultData CreateWeakDamageResult(double baseDamage)
    {
        return new DamageResultData(Convert.ToInt32(Math.Floor(baseDamage * WEAK_DAMAGE_FACTOR)), DamageType.Weak);
    }

    private DamageResultData CreateNullDamageResult()
    {
        return new DamageResultData(0, DamageType.Null);
    }

    private DamageResultData CreateRepelDamageResult(double baseDamage)
    {
        return new DamageResultData(Convert.ToInt32(Math.Floor(baseDamage)), DamageType.Repel);
    }

    private DamageResultData CreateDrainDamageResult(double baseDamage)
    {
        return new DamageResultData(Convert.ToInt32(Math.Floor(baseDamage)), DamageType.Drain);
    }

    private DamageResultData CreateNormalDamageResult(double baseDamage)
    {
        return new DamageResultData(Convert.ToInt32(Math.Floor(baseDamage)), DamageType.Normal);
    }
}