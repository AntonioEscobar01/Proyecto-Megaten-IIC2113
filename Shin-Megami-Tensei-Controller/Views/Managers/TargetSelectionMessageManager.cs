namespace Shin_Megami_Tensei;

public class TargetSelectionMessageManager
{
    private readonly GameUi _gameUi;

    public TargetSelectionMessageManager(GameUi gameUi)
    {
        _gameUi = gameUi;
    }

    public void ShowTargetSelectionPrompt(string attackerName)
    {
        _gameUi.WriteLine($"Seleccione un objetivo para {attackerName}");
    }

    public void ShowTargetCancelOption(int targetCount)
    {
        _gameUi.WriteLine($"{targetCount + 1}-Cancelar");
    }

    public void ShowSamuraiTargetOption(int index, UnitDisplayInfo unitInfo)
    {
        _gameUi.WriteLine($"{index+1}-{unitInfo.Name} HP:{unitInfo.Health.Current}/{unitInfo.Health.Max} MP:{unitInfo.Mana.Current}/{unitInfo.Mana.Max}");
    }

    public void ShowMonsterTargetOption(int index, UnitDisplayInfo unitInfo)
    {
        _gameUi.WriteLine($"{index+1}-{unitInfo.Name} HP:{unitInfo.Health.Current}/{unitInfo.Health.Max} MP:{unitInfo.Mana.Current}/{unitInfo.Mana.Max}");
    }

    public void DisplayTargets(List<IUnit> availableTargets)
    {
        for (int targetIndex = 0; targetIndex < availableTargets.Count; targetIndex++)
        {
            DisplayTarget(availableTargets[targetIndex], targetIndex);
        }
        ShowTargetCancelOption(availableTargets.Count);
    }

    private void DisplayTarget(IUnit target, int targetIndex)
    {
        if (target is Samurai samurai)
        {
            var healthInfo = new HealthInfo(samurai.GetCurrentHp(), samurai.GetMaxHp());
            var manaInfo = new ManaInfo(samurai.GetCurrentMp(), samurai.GetMaxMp());
            var unitInfo = new UnitDisplayInfo(samurai.GetName(), healthInfo, manaInfo);
            ShowSamuraiTargetOption(targetIndex, unitInfo);
        }
        else if (target is Monster monster)
        {
            var healthInfo = new HealthInfo(monster.GetCurrentHp(), monster.GetMaxHp());
            var manaInfo = new ManaInfo(monster.GetCurrentMp(), monster.GetMaxMp());
            var unitInfo = new UnitDisplayInfo(monster.GetName(), healthInfo, manaInfo);
            ShowMonsterTargetOption(targetIndex, unitInfo);
        }
    }
}