namespace Shin_Megami_Tensei;

public class ActionConstants
{
    public const int AttackAction = 1;
    public const int ShootAction = 2;
    public const int SkillAction = 3;
    public const int InvokeAction = 4;
    public const int PassTurnAction = 5;
    public const int SurrenderAction = 6;

    public const int MonsterActionOffset = 10;

    public const int AttackActionMonster = AttackAction + MonsterActionOffset;
    public const int SkillActionMonster = 12;
    public const int InvokeActionMonster = 13;
    public const int PassTurnActionMonster = 14;

    public const int CancelTargetSelection = 5;
    public const int CancelInvokeSelection = 4;
    
    // Propiedades para acceso desde instancias
    public int MonsterOffset => MonsterActionOffset;
}