namespace Shin_Megami_Tensei;

public class AttackProcessor
{
    private const int ATTACK_DAMAGE_MODIFIER = 54;
    private const int SHOOT_DAMAGE_MODIFIER = 80;
    private readonly GameUi _gameUi;
    private const string PHYS_TYPE = "Phys";
    private const string GUN_TYPE = "Gun";
    private const string RESIST_AFFINITY = "Rs";
    private const string WEAK_AFFINITY = "Wk";
    private const string NULL_AFFINITY = "Nu";
    private const string REPEL_AFFINITY = "Rp";
    private const string DRAIN_AFFINITY = "Dr";
    
    private const double RESIST_DAMAGE_FACTOR = 0.5;
    private const double WEAK_DAMAGE_FACTOR = 1.5;

    public AttackProcessor(GameUi gameUi)
    {
        _gameUi = gameUi;
    }

    public void Attack(IUnit attacker, IUnit target)
    {
        double baseDamage = CalculateDamage(attacker, ATTACK_DAMAGE_MODIFIER);
        var attackContext = new AttackContext(attacker, target, baseDamage, PHYS_TYPE, "ataca");
        ProcessAttackWithAffinity(attackContext);
    }

    public void Shoot(IUnit attacker, IUnit target)
    {
        double baseDamage = CalculateDamage(attacker, SHOOT_DAMAGE_MODIFIER);
        var attackContext = new AttackContext(attacker, target, baseDamage, GUN_TYPE, "dispara");
        ProcessAttackWithAffinity(attackContext);
    }

    private void ProcessAttackWithAffinity(AttackContext context)
    {
        string attackerName = context.Attacker.Name;
        string targetName = context.Target.Name;
        string affinity = context.Target.GetAffinity(context.AttackType);
    
        _gameUi.ShowAttack(attackerName, context.ActionType, targetName);
    
        var damageResult = CalculateDamageByAffinity(context.BaseDamage, affinity);
        var damageContext = new DamageApplicationContext(context.Attacker, context.Target, damageResult, affinity);
        ApplyDamageBasedOnAffinity(damageContext);
    
        var resultContext = new AttackResultContext(context.Attacker, context.Target, attackerName, targetName, affinity);
        ShowDamageResultForAttack(resultContext);
    }

    private DamageResultData CalculateDamageByAffinity(double baseDamage, string affinity)
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

    private void ApplyDamageBasedOnAffinity(DamageApplicationContext context)
    {
        string attackerName = context.Attacker.Name;
        string targetName = context.Target.Name;
        
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
        var affinityInfo = new AffinityResponseInfo(attackerName, context.Affinity, targetName, context.DamageResult.Amount);
        _gameUi.ShowAffinityResponse(affinityInfo);
    }

    private void ShowDamageResultForAttack(AttackResultContext context)
    {
        if (ShouldShowAttackerResult(context.Affinity))
        {
            ShowUnitHealthStatus(context.Attacker, context.AttackerName);
        }
        else
        {
            ShowUnitHealthStatus(context.Target, context.TargetName);
        }
    }

    private bool ShouldShowAttackerResult(string affinity)
    {
        return affinity == REPEL_AFFINITY;
    }

    private void ShowUnitHealthStatus(IUnit unit, string unitName)
    {
        _gameUi.ShowHpResult(unitName, unit.Hp, unit.OriginalHp);
    }

    private double CalculateDamage(IUnit attacker, int modifier)
    {
        int baseStat = GetAttackerBaseStat(attacker, modifier);
        return baseStat * modifier * 0.0114;
    }

    private int GetAttackerBaseStat(IUnit attacker, int modifier)
    {
        return IsAttackModifier(modifier) ? attacker.Str : attacker.Skl;
    }

    private bool IsAttackModifier(int modifier)
    {
        return modifier == ATTACK_DAMAGE_MODIFIER;
    }
    
    public string ApplyOffensiveSkill(IUnit attacker, IUnit target, OffensiveSkillInfo skillInfo)
    {
        string affinity = target.GetAffinity(skillInfo.Skill.type);
        int numberOfHits = skillInfo.Skill.GetHitsCount(skillInfo.UsedSkillsCount);
        double baseDamage = CalculateSkillDamage(attacker, skillInfo.Skill);

        var skillContext = new SkillExecutionContext(attacker, target, skillInfo.Skill, affinity, baseDamage, numberOfHits);
        ExecuteSkillHits(skillContext);
        ShowFinalSkillResult(attacker, target, affinity);

        return affinity;
    }

    private void ExecuteSkillHits(SkillExecutionContext context)
    {
        for (int i = 0; i < context.NumberOfHits; i++)
        {
            var hitContext = new SingleHitContext(context.Attacker, context.Target, context.Skill, context.Affinity, context.BaseDamage);
            ExecuteSingleSkillHit(hitContext);
        }
    }

    private void ExecuteSingleSkillHit(SingleHitContext hitContext)
    {
        ShowSkillAttackMessage(hitContext.Attacker, hitContext.Target, hitContext.Skill);
        var damageResult = CalculateDamageByAffinity(hitContext.BaseDamage, hitContext.Affinity);
        var damageContext = new DamageApplicationContext(hitContext.Attacker, hitContext.Target, damageResult, hitContext.Affinity);
        ApplyDamageBasedOnAffinity(damageContext);
    }

    private void ShowSkillAttackMessage(IUnit attacker, IUnit target, SkillData skill)
    {
        string attackerName = attacker.Name;
        string targetName = target.Name;
        string actionVerb = skill.GetSkillActionVerb(skill.type);
        _gameUi.WriteLine($"{attackerName} {actionVerb} {targetName}");
    }

    private void ShowFinalSkillResult(IUnit attacker, IUnit target, string affinity)
    {
        string attackerName = attacker.Name;
        string targetName = target.Name;

        if (ShouldShowAttackerResult(affinity))
        {
            ShowUnitHealthStatus(attacker, attackerName);
        }
        else
        {
            ShowUnitHealthStatus(target, targetName);
        }
    }

    private double CalculateSkillDamage(IUnit attacker, SkillData skill)
    {
        int baseStat = GetSkillBaseStat(attacker, skill);
        return Math.Sqrt(baseStat * skill.power);
    }

    private int GetSkillBaseStat(IUnit attacker, SkillData skill)
    {
        return IsPhysicalOrGunSkill(skill.type)
            ? GetPhysicalSkillBaseStat(attacker, skill.type)
            : attacker.Mag;
    }

    private bool IsPhysicalOrGunSkill(string skillType)
    {
        return skillType == "Phys" || skillType == "Gun";
    }

    private int GetPhysicalSkillBaseStat(IUnit attacker, string skillType)
    {
        return IsPhysicalSkill(skillType) ? attacker.Str : attacker.Skl;
    }

    private bool IsPhysicalSkill(string skillType)
    {
        return skillType == "Phys";
    }
}