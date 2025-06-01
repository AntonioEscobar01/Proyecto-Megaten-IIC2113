namespace Shin_Megami_Tensei;

public class DamageApplicator
{
    private readonly GameUiFacade _gameUi;

    public DamageApplicator(GameUiFacade gameUi)
    {
        _gameUi = gameUi;
    }

    public void ApplyDamageBasedOnAffinity(DamageApplicationContext context)
    {
        string attackerName = context.Attacker.GetName();
        string targetName = context.Target.GetName();
        
        ApplyDamageByType(context);
        ShowAffinityResponse(context, attackerName, targetName);
    }

    public void ShowUnitHealthStatus(IUnit unit, string unitName)
    {
        _gameUi.ShowHpResult(unitName, unit.GetCurrentHp(), unit.GetMaxHp());
    }

    private void ApplyDamageByType(DamageApplicationContext context)
    {
        switch (context.DamageResult.Type)
        {
            case DamageType.Resist:
            case DamageType.Weak:
            case DamageType.Normal:
                context.Target.TakeDamage(context.DamageResult.Amount);
                break;
            case DamageType.Null:
                break;
            case DamageType.Repel:
                context.Attacker.TakeDamage(context.DamageResult.Amount);
                break;
            case DamageType.Drain:
                context.Target.Heal(context.DamageResult.Amount);
                break;
        }
    }

    private void ShowAffinityResponse(DamageApplicationContext context, string attackerName, string targetName)
    {
        var names = new CombatNames(attackerName, targetName);
        var responseData = new AffinityResponseData(context.Affinity, context.DamageResult.Amount);
        var affinityInfo = new AffinityResponseInfo(names, responseData);
        _gameUi.ShowAffinityResponse(affinityInfo);
    }
}