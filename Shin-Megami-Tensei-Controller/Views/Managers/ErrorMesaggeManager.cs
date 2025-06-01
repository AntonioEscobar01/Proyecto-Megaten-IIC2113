namespace Shin_Megami_Tensei;

public class ErrorMessageManager
{
    private readonly GameUi _gameUi;

    public ErrorMessageManager(GameUi gameUi)
    {
        _gameUi = gameUi;
    }

    public void ShowInvalidTeamFileError()
    {
        _gameUi.WriteLine("Archivo de equipos inválido");
    }

    public void ShowFileSelectionPrompt()
    {
        _gameUi.WriteLine("Elige un archivo para cargar los equipos");
    }
}