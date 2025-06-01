namespace Shin_Megami_Tensei;

public class AllySelectionMessageManager
{
    private readonly GameUi _gameUi;

    public AllySelectionMessageManager(GameUi gameUi)
    {
        _gameUi = gameUi;
    }

    public void ShowAllySelectionPrompt(string healerName)
    {
        _gameUi.WriteLine($"Seleccione un objetivo para {healerName}");
    }

    public void ShowAllyCancelOption(int allyCount)
    {
        _gameUi.WriteLine($"{allyCount + 1}-Cancelar");
    }

    public void ShowSamuraiAllyOption(int index, UnitDisplayInfo unitInfo)
    {
        _gameUi.WriteLine($"{index+1}-{unitInfo.Name} HP:{unitInfo.Health.Current}/{unitInfo.Health.Max} MP:{unitInfo.Mana.Current}/{unitInfo.Mana.Max}");
    }

    public void ShowMonsterAllyOption(int index, UnitDisplayInfo unitInfo)
    {
        _gameUi.WriteLine($"{index+1}-{unitInfo.Name} HP:{unitInfo.Health.Current}/{unitInfo.Health.Max} MP:{unitInfo.Mana.Current}/{unitInfo.Mana.Max}");
    }

    public void DisplayAllies(List<IUnit> availableAllies)
    {
        for (int allyIndex = 0; allyIndex < availableAllies.Count; allyIndex++)
        {
            DisplayAlly(availableAllies[allyIndex], allyIndex);
        }
        ShowAllyCancelOption(availableAllies.Count);
    }

    private void DisplayAlly(IUnit ally, int allyIndex)
    {
        if (ally is Samurai samurai)
        {
            DisplaySamuraiInfo(samurai, allyIndex);
        }
        else if (ally is Monster monster)
        {
            DisplayMonsterInfo(monster, allyIndex);
        }
    }

    private void DisplaySamuraiInfo(Samurai samurai, int allyIndex)
    {
        var healthInfo = new HealthInfo(samurai.GetCurrentHp(), samurai.GetMaxHp());
        var manaInfo = new ManaInfo(samurai.GetCurrentMp(), samurai.GetMaxMp());
        var unitInfo = new UnitDisplayInfo(samurai.GetName(), healthInfo, manaInfo);
        ShowSamuraiAllyOption(allyIndex, unitInfo);
    }

    private void DisplayMonsterInfo(Monster monster, int allyIndex)
    {
        var healthInfo = new HealthInfo(monster.GetCurrentHp(), monster.GetMaxHp());
        var manaInfo = new ManaInfo(monster.GetCurrentMp(), monster.GetMaxMp());
        var unitInfo = new UnitDisplayInfo(monster.GetName(), healthInfo, manaInfo);
        ShowMonsterAllyOption(allyIndex, unitInfo);
    }
}