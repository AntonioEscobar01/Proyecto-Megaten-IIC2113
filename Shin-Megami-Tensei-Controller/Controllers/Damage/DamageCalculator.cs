namespace Shin_Megami_Tensei;

public class DamageCalculator
{
    private const int ATTACK_DAMAGE_MODIFIER = 54;
    private const int SHOOT_DAMAGE_MODIFIER = 80;

    public double CalculateBasicAttackDamage(IUnit attacker)
    {
        return CalculateDamage(attacker, ATTACK_DAMAGE_MODIFIER);
    }

    public double CalculateShootDamage(IUnit attacker)
    {
        return CalculateDamage(attacker, SHOOT_DAMAGE_MODIFIER);
    }

    public double CalculateSkillDamage(IUnit attacker, SkillData skill)
    {
        int baseStat = GetSkillBaseStat(attacker, skill);
        return Math.Sqrt(baseStat * skill.power);
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

    private int GetPhysicalSkillBaseStat(IUnit attacker, string skillType)
    {
        return IsPhysicalSkill(skillType) ? attacker.GetStr() : attacker.GetSkl();
    }

    private bool IsPhysicalSkill(string skillType)
    {
        return skillType == "Phys";
    }
}