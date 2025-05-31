namespace Shin_Megami_Tensei;

public class AttackProcessor
{
    private const int ATTACK_DAMAGE_MODIFIER = 54;
    private const int SHOOT_DAMAGE_MODIFIER = 80;
    private readonly GameUi _gameUi;
    
    // Tipos de ataque
    private const string PHYS_TYPE = "Phys";
    private const string GUN_TYPE = "Gun";
    
    // Afinidades
    private const string RESIST_AFFINITY = "Rs";
    private const string WEAK_AFFINITY = "Wk";
    private const string NULL_AFFINITY = "Nu";
    private const string REPEL_AFFINITY = "Rp";
    private const string DRAIN_AFFINITY = "Dr";
    
    // Multiplicadores de daño
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
            RESIST_AFFINITY => new DamageResultData(Convert.ToInt32(Math.Floor(baseDamage * RESIST_DAMAGE_FACTOR)), DamageType.Resist),
            WEAK_AFFINITY => new DamageResultData(Convert.ToInt32(Math.Floor(baseDamage * WEAK_DAMAGE_FACTOR)), DamageType.Weak),
            NULL_AFFINITY => new DamageResultData(0, DamageType.Null),
            REPEL_AFFINITY => new DamageResultData(Convert.ToInt32(Math.Floor(baseDamage)), DamageType.Repel),
            DRAIN_AFFINITY => new DamageResultData(Convert.ToInt32(Math.Floor(baseDamage)), DamageType.Drain),
            _ => new DamageResultData(Convert.ToInt32(Math.Floor(baseDamage)), DamageType.Normal)
        };
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
                UpdateTargetHealth(context.Target, context.DamageResult.Amount);
                break;
            case DamageType.Null:
                // No damage applied
                break;
            case DamageType.Repel:
                UpdateTargetHealth(context.Attacker, context.DamageResult.Amount);
                break;
            case DamageType.Drain:
                HealTarget(context.Target, context.DamageResult.Amount);
                break;
        }
        var affinityInfo = new AffinityResponseInfo(attackerName, context.Affinity, targetName, context.DamageResult.Amount);
        _gameUi.ShowAffinityResponse(affinityInfo);
    }
    

    private void ShowDamageResultForAttack(AttackResultContext context)
    {
        if (context.Affinity == REPEL_AFFINITY)
        {
            ShowUnitHealthStatus(context.Attacker, context.AttackerName);
        }
        else
        {
            ShowUnitHealthStatus(context.Target, context.TargetName);
        }
    }

    private void ShowUnitHealthStatus(object unit, string unitName)
    {
        int remainingHp = GetUnitHp(unit);
        int originalHp = GetUnitOriginalHp(unit);
        _gameUi.ShowHpResult(unitName, remainingHp, originalHp);
    }

    private string GetTargetAffinity(object target, string attackType)
    {
        if (target is Samurai samurai)
        {
            return samurai.Affinities.GetAffinity(attackType);
        }
        else if (target is Monster monster)
        {
            return monster.Affinities.GetAffinity(attackType);
        }
        
        return "-";
    }

    private void UpdateTargetHealth(object target, int damage)
    {
        if (target is Samurai samurai)
        {
            samurai.Hp -= damage;
            if (samurai.Hp < 0) samurai.Hp = 0;
        }
        else if (target is Monster monster)
        {
            monster.Hp -= damage;
            if (monster.Hp < 0) monster.Hp = 0;
        }
    }
    
    private void HealTarget(object target, int healAmount)
    {
        if (target is Samurai samurai)
        {
            samurai.Hp += healAmount;
            if (samurai.Hp > samurai.OriginalHp) samurai.Hp = samurai.OriginalHp;
        }
        else if (target is Monster monster)
        {
            monster.Hp += healAmount;
            if (monster.Hp > monster.OriginalHp) monster.Hp = monster.OriginalHp;
        }
    }

    private double CalculateDamage(object attacker, int modifier)
    {
        int baseStat = attacker switch
        {
            Samurai samurai => modifier == ATTACK_DAMAGE_MODIFIER ? samurai.Str : samurai.Skl,
            Monster monster => modifier == ATTACK_DAMAGE_MODIFIER ? monster.Str : monster.Skl,
            _ => 0
        };
        return baseStat * modifier * 0.0114;
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
    
    public string ApplyOffensiveSkill(object attacker, object target, SkillData skill, int usedSkillsCount)
    {
        string affinity = GetTargetAffinity(target, skill.type);
        int numberOfHits = skill.GetHitsCount(usedSkillsCount);
        double baseDamage = CalculateSkillDamage(attacker, skill);

        var skillContext = new SkillExecutionContext(attacker, target, skill, affinity, baseDamage, numberOfHits);
        ExecuteSkillHits(skillContext);
        ShowFinalSkillResult(attacker, target, affinity);

        return affinity;
    }

    private void ExecuteSkillHits(SkillExecutionContext context)
    {
        for (int i = 0; i < context.NumberOfHits; i++)
        {
            ExecuteSingleSkillHit(context.Attacker, context.Target, context.Skill, context.Affinity, context.BaseDamage);
        }
    }

    private void ExecuteSingleSkillHit(object attacker, object target, SkillData skill, string affinity, double baseDamage)
    {
        ShowSkillAttackMessage(attacker, target, skill);
        var damageResult = CalculateDamageByAffinity(baseDamage, affinity);
        var damageContext = new DamageApplicationContext(attacker, target, damageResult, affinity);
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

        if (affinity == REPEL_AFFINITY)
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
        int baseStat = GetBaseStat(attacker, skill);
        return Math.Sqrt(baseStat * skill.power);
    }

    private int GetBaseStat(object attacker, SkillData skill)
    {
        if (IsPhysicalOrGunSkill(skill.type))
        {
            return GetPhysicalBaseStat(attacker, skill.type);
        }
        else
        {
            return GetMagicalBaseStat(attacker);
        }
    }

    private bool IsPhysicalOrGunSkill(string skillType)
    {
        return skillType == "Phys" || skillType == "Gun";
    }

    private int GetPhysicalBaseStat(object attacker, string skillType)
    {
        return attacker switch
        {
            Samurai samurai => skillType == "Phys" ? samurai.Str : samurai.Skl,
            Monster monster => skillType == "Phys" ? monster.Str : monster.Skl,
            _ => 0
        };
    }

    private int GetMagicalBaseStat(object attacker)
    {
        return attacker switch
        {
            Samurai samurai => samurai.Mag,
            Monster monster => monster.Mag,
            _ => 0
        };
    }
}