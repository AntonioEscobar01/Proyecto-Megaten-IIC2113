namespace Shin_Megami_Tensei;

public class BattleSystem
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

    public BattleSystem(GameUi gameUi)
    {
        _gameUi = gameUi;
    }

    public void Attack(object attacker, object target)
    {
        int baseDamage = CalculateDamage(attacker, ATTACK_DAMAGE_MODIFIER);
        ProcessAttackWithAffinity(attacker, target, baseDamage, PHYS_TYPE, "ataca");
    }

    public void Shoot(object attacker, object target)
    {
        int baseDamage = CalculateDamage(attacker, SHOOT_DAMAGE_MODIFIER);
        ProcessAttackWithAffinity(attacker, target, baseDamage, GUN_TYPE, "dispara");
    }

    private void ProcessAttackWithAffinity(object attacker, object target, int baseDamage, string attackType, string actionType)
    {
        string attackerName = _gameUi.GetUnitName(attacker);
        string targetName = _gameUi.GetUnitName(target);
        string affinity = GetTargetAffinity(target, attackType);
        
        _gameUi.ShowAttack(attackerName, actionType, targetName);
        
        switch (affinity)
        {
            case RESIST_AFFINITY:
                int resistDamage = (int)(baseDamage * RESIST_DAMAGE_FACTOR);
                UpdateTargetHealth(target, resistDamage);
                _gameUi.ShowAffinityResponse(attackerName, affinity, targetName, resistDamage);
                break;
                
            case WEAK_AFFINITY:
                int weakDamage = (int)(baseDamage * WEAK_DAMAGE_FACTOR);
                UpdateTargetHealth(target, weakDamage);
                _gameUi.ShowAffinityResponse(attackerName, affinity, targetName, weakDamage);
                break;
                
            case NULL_AFFINITY:
                _gameUi.ShowAffinityResponse(attackerName, affinity, targetName, 0);
                break;
                
            case REPEL_AFFINITY:
                UpdateTargetHealth(attacker, baseDamage);
                _gameUi.ShowAffinityResponse(attackerName, affinity, targetName, baseDamage);
                break;
                
            case DRAIN_AFFINITY:
                HealTarget(target, baseDamage);
                _gameUi.ShowAffinityResponse(attackerName, affinity, targetName, baseDamage);
                break;
                
            default:
                UpdateTargetHealth(target, baseDamage);
                _gameUi.ShowAffinityResponse(attackerName, affinity, targetName, baseDamage);
                break;
        }
        
        int remainingHp = GetUnitHp(target);
        int originalHp = GetUnitOriginalHp(target);
        
        _gameUi.ShowDamageResult(targetName, remainingHp, originalHp);
        
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

    private int CalculateDamage(object attacker, int modifier)
    {
        int baseStat = attacker switch
        {
            Samurai samurai => modifier == ATTACK_DAMAGE_MODIFIER ? samurai.Str : samurai.Skl,
            Monster monster => modifier == ATTACK_DAMAGE_MODIFIER ? monster.Str : monster.Skl,
            _ => 0
        };
        return Convert.ToInt32(Math.Floor(baseStat * modifier * 0.0114));
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
}