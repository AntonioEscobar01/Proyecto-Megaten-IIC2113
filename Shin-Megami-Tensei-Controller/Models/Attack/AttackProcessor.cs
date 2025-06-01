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

    public void Attack(object attacker, object target)
    {
        double baseDamage = CalculateDamage(attacker, ATTACK_DAMAGE_MODIFIER);
        var attackContext = new AttackContext(attacker, target, baseDamage, PHYS_TYPE, "ataca");
        ProcessAttackWithAffinity(attackContext);
    }

    public void Shoot(object attacker, object target)
    {
        double baseDamage = CalculateDamage(attacker, SHOOT_DAMAGE_MODIFIER);
        var attackContext = new AttackContext(attacker, target, baseDamage, GUN_TYPE, "dispara");
        ProcessAttackWithAffinity(attackContext);
    }

    private void ProcessAttackWithAffinity(AttackContext context)
    {
        string attackerName = _gameUi.GetUnitName(context.Attacker);
        string targetName = _gameUi.GetUnitName(context.Target);
        string affinity = GetTargetAffinity(context.Target, context.AttackType);
    
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
        string attackerName = _gameUi.GetUnitName(context.Attacker);
        string targetName = _gameUi.GetUnitName(context.Target);
        
        switch (context.DamageResult.Type)
        {
            case DamageType.Resist:
            case DamageType.Weak:
            case DamageType.Normal:
                ApplyDamageToTarget(context.Target, context.DamageResult.Amount);
                break;
            case DamageType.Null:
                break;
            case DamageType.Repel:
                ApplyDamageToTarget(context.Attacker, context.DamageResult.Amount);
                break;
            case DamageType.Drain:
                HealTarget(context.Target, context.DamageResult.Amount);
                break;
        }
        var affinityInfo = new AffinityResponseInfo(attackerName, context.Affinity, targetName, context.DamageResult.Amount);
        _gameUi.ShowAffinityResponse(affinityInfo);
    }

    private void ApplyDamageToTarget(object target, int damage)
    {
        UpdateTargetHealth(target, damage);
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

    private void ShowUnitHealthStatus(object unit, string unitName)
    {
        int remainingHp = GetUnitHp(unit);
        int originalHp = GetUnitOriginalHp(unit);
        _gameUi.ShowHpResult(unitName, remainingHp, originalHp);
    }

    private string GetTargetAffinity(object target, string attackType)
    {
        return target switch
        {
            Samurai samurai => GetSamuraiAffinity(samurai, attackType),
            Monster monster => GetMonsterAffinity(monster, attackType),
            _ => "-"
        };
    }

    private string GetSamuraiAffinity(Samurai samurai, string attackType)
    {
        return samurai.Affinities.GetAffinity(attackType);
    }

    private string GetMonsterAffinity(Monster monster, string attackType)
    {
        return monster.Affinities.GetAffinity(attackType);
    }

    private void UpdateTargetHealth(object target, int damage)
    {
        switch (target)
        {
            case Samurai samurai:
                ApplyDamageToSamurai(samurai, damage);
                break;
            case Monster monster:
                ApplyDamageToMonster(monster, damage);
                break;
        }
    }

    private void ApplyDamageToSamurai(Samurai samurai, int damage)
    {
        samurai.Hp -= damage;
        if (IsHealthBelowZero(samurai.Hp)) 
            samurai.Hp = 0;
    }

    private void ApplyDamageToMonster(Monster monster, int damage)
    {
        monster.Hp -= damage;
        if (IsHealthBelowZero(monster.Hp)) 
            monster.Hp = 0;
    }

    private bool IsHealthBelowZero(int hp)
    {
        return hp < 0;
    }
    
    private void HealTarget(object target, int healAmount)
    {
        switch (target)
        {
            case Samurai samurai:
                ApplyHealingToSamurai(samurai, healAmount);
                break;
            case Monster monster:
                ApplyHealingToMonster(monster, healAmount);
                break;
        }
    }

    private void ApplyHealingToSamurai(Samurai samurai, int healAmount)
    {
        samurai.Hp += healAmount;
        if (ExceedsMaxHp(samurai.Hp, samurai.OriginalHp)) 
            samurai.Hp = samurai.OriginalHp;
    }

    private void ApplyHealingToMonster(Monster monster, int healAmount)
    {
        monster.Hp += healAmount;
        if (ExceedsMaxHp(monster.Hp, monster.OriginalHp)) 
            monster.Hp = monster.OriginalHp;
    }

    private bool ExceedsMaxHp(int currentHp, int maxHp)
    {
        return currentHp > maxHp;
    }

    private double CalculateDamage(object attacker, int modifier)
    {
        int baseStat = GetAttackerBaseStat(attacker, modifier);
        return baseStat * modifier * 0.0114;
    }

    private int GetAttackerBaseStat(object attacker, int modifier)
    {
        return attacker switch
        {
            Samurai samurai => GetSamuraiBaseStat(samurai, modifier),
            Monster monster => GetMonsterBaseStat(monster, modifier),
            _ => 0
        };
    }

    private int GetSamuraiBaseStat(Samurai samurai, int modifier)
    {
        return IsAttackModifier(modifier) ? samurai.Str : samurai.Skl;
    }

    private int GetMonsterBaseStat(Monster monster, int modifier)
    {
        return IsAttackModifier(modifier) ? monster.Str : monster.Skl;
    }

    private bool IsAttackModifier(int modifier)
    {
        return modifier == ATTACK_DAMAGE_MODIFIER;
    }
    
    private int GetUnitHp(object unit)
    {
        return unit switch
        {
            Samurai samurai => samurai.Hp,
            Monster monster => monster.Hp,
            _ => 0
        };
    }
    
    private int GetUnitOriginalHp(object unit)
    {
        return unit switch
        {
            Samurai samurai => samurai.OriginalHp,
            Monster monster => monster.OriginalHp,
            _ => 0
        };
    }
    
    public string ApplyOffensiveSkill(object attacker, object target, OffensiveSkillInfo skillInfo)
    {
        string affinity = GetTargetAffinity(target, skillInfo.Skill.type);
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

    private void ShowSkillAttackMessage(object attacker, object target, SkillData skill)
    {
        string attackerName = _gameUi.GetUnitName(attacker);
        string targetName = _gameUi.GetUnitName(target);
        string actionVerb = skill.GetSkillActionVerb(skill.type);
        _gameUi.WriteLine($"{attackerName} {actionVerb} {targetName}");
    }

    private void ShowFinalSkillResult(object attacker, object target, string affinity)
    {
        string attackerName = _gameUi.GetUnitName(attacker);
        string targetName = _gameUi.GetUnitName(target);

        if (ShouldShowAttackerResult(affinity))
        {
            ShowUnitHealthStatus(attacker, attackerName);
        }
        else
        {
            ShowUnitHealthStatus(target, targetName);
        }
    }

    private double CalculateSkillDamage(object attacker, SkillData skill)
    {
        int baseStat = GetSkillBaseStat(attacker, skill);
        return Math.Sqrt(baseStat * skill.power);
    }

    private int GetSkillBaseStat(object attacker, SkillData skill)
    {
        return IsPhysicalOrGunSkill(skill.type)
            ? GetPhysicalSkillBaseStat(attacker, skill.type)
            : GetMagicalSkillBaseStat(attacker);
    }

    private bool IsPhysicalOrGunSkill(string skillType)
    {
        return skillType == "Phys" || skillType == "Gun";
    }

    private int GetPhysicalSkillBaseStat(object attacker, string skillType)
    {
        return attacker switch
        {
            Samurai samurai => GetSamuraiPhysicalStat(samurai, skillType),
            Monster monster => GetMonsterPhysicalStat(monster, skillType),
            _ => 0
        };
    }

    private int GetSamuraiPhysicalStat(Samurai samurai, string skillType)
    {
        return IsPhysicalSkill(skillType) ? samurai.Str : samurai.Skl;
    }

    private int GetMonsterPhysicalStat(Monster monster, string skillType)
    {
        return IsPhysicalSkill(skillType) ? monster.Str : monster.Skl;
    }

    private bool IsPhysicalSkill(string skillType)
    {
        return skillType == "Phys";
    }

    private int GetMagicalSkillBaseStat(object attacker)
    {
        return attacker switch
        {
            Samurai samurai => samurai.Mag,
            Monster monster => monster.Mag,
            _ => 0
        };
    }
}