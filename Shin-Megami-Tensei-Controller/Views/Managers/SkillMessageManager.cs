namespace Shin_Megami_Tensei;

public class SkillMessageManager
{
    private readonly GameUi _gameUi;
    private readonly GameUiUtilities _utilities;

    public SkillMessageManager(GameUi gameUi)
    {
        _gameUi = gameUi;
        _utilities = new GameUiUtilities();
    }

    public void ShowHealMessage(IUnit target, int healAmount)
    {
        string targetName = _utilities.GetUnitName(target);
        _gameUi.WriteLine($"{targetName} recibe {healAmount} de HP");
    }

    public void ShowHealingAction(string healerName, string targetName)
    {
        _gameUi.WriteLine($"{healerName} cura a {targetName}");
    }

    public void ShowHealAmountReceived(string targetName, int healAmount)
    {
        _gameUi.WriteLine($"{targetName} recibe {healAmount} de HP");
    }

    public void ShowReviveAction(string reviverName, string targetName)
    {
        _gameUi.WriteLine($"{reviverName} revive a {targetName}");
    }

    public void ShowSkillSelectionPrompt(string unitName)
    {
        _gameUi.WriteLine($"Seleccione una habilidad para que {unitName} use");
    }

    public void ShowAffordableSkill(int optionNumber, string skillName, int skillCost)
    {
        _gameUi.WriteLine($"{optionNumber}-{skillName} MP:{skillCost}");
    }

    public void ShowSkillCancelOption(int optionNumber)
    {
        _gameUi.WriteLine($"{optionNumber}-Cancelar");
    }

    public void DisplaySummonSuccess(string monsterName)
    {
        _gameUi.WriteLine($"{monsterName} ha sido invocado");
    }

    public void ShowSurrenderMessage(string samuraiName, string playerName)
    {
        _gameUi.WriteLine($"{samuraiName} ({playerName}) se rinde");
    }

    public void ShowChargeMessage(string casterName)
    {
        _gameUi.WriteLine($"{casterName} ha cargado su siguiente ataque físico o disparo a más del doble");
    }

    public void ShowConcentrateMessage(string casterName)
    {
        _gameUi.WriteLine($"{casterName} ha cargado su siguiente ataque mágico a más del doble");
    }

    public void ShowBloodRitualMessage(string casterName)
    {
        _gameUi.WriteLine($"El ataque de {casterName} ha aumentado");
        _gameUi.WriteLine($"La defensa de {casterName} ha aumentado");
    }
}