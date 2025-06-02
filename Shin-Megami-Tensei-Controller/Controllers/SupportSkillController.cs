namespace Shin_Megami_Tensei;

public class SupportSkillController
{
    private readonly GameUiFacade _gameUi;

    public SupportSkillController(GameUiFacade gameUi)
    {
        _gameUi = gameUi;
    }

    public void ExecuteSupportSkill(IUnit caster, SkillData skillData)
    {
        string skillName = skillData.name;
        string casterName = caster.GetName();

        switch (skillName)
        {
            case "Charge":
            case "Dark Energy":
                ExecuteChargeEffect(caster, casterName);
                break;
            case "Concentrate":
            case "Gather Spirit Energy":
                ExecuteConcentrateEffect(caster, casterName);
                break;
            case "Blood Ritual":
                ExecuteBloodRitualEffect(caster, casterName);
                break;
        }
    }

    public bool CanUseSupportSkill(IUnit caster, SkillData skillData)
    {
        if (skillData.name == "Blood Ritual")
        {
            return caster.GetCurrentHp() >= 2;
        }
        return true;
    }

    private void ExecuteChargeEffect(IUnit caster, string casterName)
    {
        caster.GetSupportEffects().ActivateCharge();
        _gameUi.ShowChargeMessage(casterName);
    }

    private void ExecuteConcentrateEffect(IUnit caster, string casterName)
    {
        caster.GetSupportEffects().ActivateConcentrate();
        _gameUi.ShowConcentrateMessage(casterName);
    }

    private void ExecuteBloodRitualEffect(IUnit caster, string casterName)
    {
        caster.GetSupportEffects().ApplyBloodRitual();
        _gameUi.ShowBloodRitualMessage(casterName);
        
        int currentHp = caster.GetCurrentHp();
        if (currentHp > 1)
        {
            caster.TakeDamage(currentHp - 1);
        }
        
        _gameUi.ShowHpResult(casterName, caster.GetCurrentHp(), caster.GetMaxHp());
    }
}