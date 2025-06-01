namespace Shin_Megami_Tensei;

public class SelectionMenuManager
{
    private readonly GameUi _gameUi;

    public SelectionMenuManager(GameUi gameUi)
    {
        _gameUi = gameUi;
    }

    public int DisplaySummonMenu(List<Monster> availableMonsters)
    {
        _gameUi.WriteLine("Seleccione un monstruo para invocar");
        DisplayAvailableMonsters(availableMonsters);
        DisplaySummonCancelOption(availableMonsters.Count);
        return int.Parse(_gameUi.ReadLine());
    }

    public int DisplayPositionMenu(Team currentTeam)
    {
        _gameUi.WriteLine("Seleccione una posición para invocar");
        DisplayPositionOptions(currentTeam);
        _gameUi.WriteLine("4-Cancelar");
        return int.Parse(_gameUi.ReadLine());
    }

    private void DisplayAvailableMonsters(List<Monster> availableMonsters)
    {
        for (int i = 0; i < availableMonsters.Count; i++)
        {
            Monster monster = availableMonsters[i];
            _gameUi.WriteLine($"{i + 1}-{monster.GetName()} HP:{monster.GetCurrentHp()}/{monster.GetMaxHp()} MP:{monster.GetCurrentMp()}/{monster.GetMaxMp()}");
        }
    }

    private void DisplaySummonCancelOption(int monstersCount)
    {
        _gameUi.WriteLine($"{monstersCount + 1}-Cancelar");
    }

    private void DisplayPositionOptions(Team currentTeam)
    {
        for (int i = 0; i < 3; i++)
        {
            DisplayPositionSlot(currentTeam, i);
        }
    }

    private void DisplayPositionSlot(Team currentTeam, int position)
    {
        if (currentTeam.IsMonsterAliveAtPosition(position))
        {
            DisplayOccupiedPosition(currentTeam, position);
        }
        else
        {
            DisplayEmptyPosition(position);
        }
    }

    private void DisplayOccupiedPosition(Team currentTeam, int position)
    {
        Monster monster = currentTeam.GetMonsterAtPosition(position);
        _gameUi.WriteLine($"{position+1}-{monster.GetName()} HP:{monster.GetCurrentHp()}/{monster.GetMaxHp()} MP:{monster.GetCurrentMp()}/{monster.GetMaxMp()} (Puesto {position+2})");
    }

    private void DisplayEmptyPosition(int position)
    {
        _gameUi.WriteLine($"{position+1}-Vacío (Puesto {position+2})");
    }
}