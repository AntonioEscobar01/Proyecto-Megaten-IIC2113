namespace Shin_Megami_Tensei;

public class BattleSystem
{
    private const int ATTACK_DAMAGE_MODIFIER = 54;
    private const int SHOOT_DAMAGE_MODIFIER = 80;
    private readonly GameUi _gameUi;

    public BattleSystem(GameUi gameUi)
    {
        _gameUi = gameUi;
    }

    public void Attack(object attacker, object target)
    {
        int damage = CalculateDamage(attacker, ATTACK_DAMAGE_MODIFIER);
        ApplyDamage(attacker, target, damage, "ataca");
    }

    public void Shoot(object attacker, object target)
    {
        int damage = CalculateDamage(attacker, SHOOT_DAMAGE_MODIFIER);
        ApplyDamage(attacker, target, damage, "dispara");
    }

    private void ApplyDamage(object attacker, object target, int damage, string actionType)
    {
        string attackerName = _gameUi.GetUnitName(attacker);
        string targetName = _gameUi.GetUnitName(target);
        
        UpdateTargetHealth(target, damage);
        
        int remainingHp = GetUnitHp(target);
        int originalHp = GetUnitOriginalHp(target);
        
        _gameUi.ShowDamageResult(attackerName, targetName, actionType, damage, remainingHp, originalHp);
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