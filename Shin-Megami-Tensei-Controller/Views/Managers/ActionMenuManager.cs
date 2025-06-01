namespace Shin_Megami_Tensei;

public class ActionMenuManager
{
    private readonly GameUi _gameUi;

    public ActionMenuManager(GameUi gameUi)
    {
        _gameUi = gameUi;
    }

    public int GetSamuraiActionOptions(Samurai samurai)
    {
        ShowSamuraiActionPrompt(samurai);
        DisplaySamuraiActionOptions();
        return int.Parse(_gameUi.ReadLine());
    }

    public int GetMonsterActionOptions(Monster monster)
    {
        ShowMonsterActionPrompt(monster);
        DisplayMonsterActionOptions();
        return int.Parse(_gameUi.ReadLine());
    }

    private void ShowSamuraiActionPrompt(Samurai samurai)
    {
        _gameUi.WriteLine($"Seleccione una acción para {samurai.GetName()}");
    }

    private void DisplaySamuraiActionOptions()
    {
        _gameUi.WriteLine("1: Atacar");
        _gameUi.WriteLine("2: Disparar");
        _gameUi.WriteLine("3: Usar Habilidad");
        _gameUi.WriteLine("4: Invocar");
        _gameUi.WriteLine("5: Pasar Turno");
        _gameUi.WriteLine("6: Rendirse");
    }

    private void ShowMonsterActionPrompt(Monster monster)
    {
        _gameUi.WriteLine($"Seleccione una acción para {monster.GetName()}");
    }

    private void DisplayMonsterActionOptions()
    {
        _gameUi.WriteLine("1: Atacar");
        _gameUi.WriteLine("2: Usar Habilidad");
        _gameUi.WriteLine("3: Invocar");
        _gameUi.WriteLine("4: Pasar Turno");
    }
}