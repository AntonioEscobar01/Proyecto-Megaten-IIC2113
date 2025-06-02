namespace Shin_Megami_Tensei;

public class DamageCalculator
{
    private const int ATTACK_DAMAGE_MODIFIER = 54;
    private const int SHOOT_DAMAGE_MODIFIER = 80;
    private const double CHARGE_MULTIPLIER = 2.5;
    private const double CONCENTRATE_MULTIPLIER = 2.5;

    public double CalculateBasicAttackDamage(IUnit attacker)
    {
        double baseDamage = CalculateDamage(attacker, ATTACK_DAMAGE_MODIFIER);
        return ApplyChargeIfActive(attacker, baseDamage);
    }

    public double CalculateShootDamage(IUnit attacker)
    {
        double baseDamage = CalculateDamage(attacker, SHOOT_DAMAGE_MODIFIER);
        return ApplyChargeIfActive(attacker, baseDamage);
    }

    public double CalculateSkillDamage(IUnit attacker, SkillData skill)
    {
        int baseStat = GetSkillBaseStat(attacker, skill);
        double baseDamage = Math.Sqrt(baseStat * skill.power);
        
        if (IsPhysicalOrGunSkill(skill.type))
        {
            baseDamage = ApplyChargeIfActive(attacker, baseDamage);
        }
        else if (IsMagicalSkill(skill.type))
        {
            baseDamage = ApplyConcentrateIfActive(attacker, baseDamage);
        }
        
        return baseDamage;
    }

    private double CalculateDamage(IUnit attacker, int modifier)
    {
        int baseStat = GetAttackerBaseStat(attacker, modifier);
        return baseStat * modifier * 0.0114;
    }

    private int GetAttackerBaseStat(IUnit attacker, int modifier)
    {
        return IsAttackModifier(modifier) ? attacker.GetStr() : attacker.GetSkl();
    }

    private bool IsAttackModifier(int modifier)
    {
        return modifier == ATTACK_DAMAGE_MODIFIER;
    }

    private int GetSkillBaseStat(IUnit attacker, SkillData skill)
    {
        return IsPhysicalOrGunSkill(skill.type)
            ? GetPhysicalSkillBaseStat(attacker, skill.type)
            : attacker.GetMag();
    }

    private bool IsPhysicalOrGunSkill(string skillType)
    {
        return skillType == "Phys" || skillType == "Gun";
    }

    private bool IsMagicalSkill(string skillType)
    {
        return skillType == "Fire" || skillType == "Ice" || 
               skillType == "Elec" || skillType == "Force";
    }

    private int GetPhysicalSkillBaseStat(IUnit attacker, string skillType)
    {
        return IsPhysicalSkill(skillType) ? attacker.GetStr() : attacker.GetSkl();
    }

    private bool IsPhysicalSkill(string skillType)
    {
        return skillType == "Phys";
    }

    private double ApplyChargeIfActive(IUnit attacker, double baseDamage)
    {
        var supportEffects = attacker.GetSupportEffects();
        if (supportEffects.IsChargeActive())
        {
            supportEffects.ConsumeCharge();
            return baseDamage * CHARGE_MULTIPLIER;
        }
        return baseDamage;
    }

    private double ApplyConcentrateIfActive(IUnit attacker, double baseDamage)
    {
        var supportEffects = attacker.GetSupportEffects();
        if (supportEffects.IsConcentrateActive())
        {
            supportEffects.ConsumeConcentrate();
            return baseDamage * CONCENTRATE_MULTIPLIER;
        }
        return baseDamage;
    }
}