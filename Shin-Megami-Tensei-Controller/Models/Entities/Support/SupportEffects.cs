namespace Shin_Megami_Tensei;

public class SupportEffects
{
    private bool _chargeActive;
    private bool _concentrateActive;
    private int _attackBonus;
    private int _defenseBonus;

    public SupportEffects()
    {
        _chargeActive = false;
        _concentrateActive = false;
        _attackBonus = 0;
        _defenseBonus = 0;
    }

    public bool IsChargeActive() => _chargeActive;
    public bool IsConcentrateActive() => _concentrateActive;
    public int GetAttackBonus() => _attackBonus;
    public int GetDefenseBonus() => _defenseBonus;

    public void ActivateCharge()
    {
        _chargeActive = true;
    }

    public void ActivateConcentrate()
    {
        _concentrateActive = true;
    }

    public void ApplyBloodRitual()
    {
        _attackBonus += 1;
        _defenseBonus += 1;
    }

    public void ConsumeCharge()
    {
        _chargeActive = false;
    }

    public void ConsumeConcentrate()
    {
        _concentrateActive = false;
    }

    public void ResetEffects()
    {
        _chargeActive = false;
        _concentrateActive = false;
        _attackBonus = 0;
        _defenseBonus = 0;
    }
}