using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class TeamLoadManager
{
    private readonly GameUi _gameUi;
    private readonly string _teamsFolder;

    public TeamLoadManager(GameUi gameUi, string teamsFolder)
    {
        _gameUi = gameUi;
        _teamsFolder = teamsFolder;
    }

    public (Team TeamPlayer1, Team TeamPlayer2, bool AreTeamsValid) LoadTeamsFromFile()
    {
        var teamFiles = Directory.GetFiles(_teamsFolder);
        _gameUi.WriteLine("Elige un archivo para cargar los equipos");
        _gameUi.ShowFiles(teamFiles);
        
        int chosenFileIndex = int.Parse(_gameUi.ReadLine());
        string chosenFile = teamFiles[chosenFileIndex];
        var teamsInfo = File.ReadAllLines(chosenFile);

        var (unitsPlayer1Names, unitsPlayer2Names) = ExtractTeamUnitNames(teamsInfo);

        var team1 = new Team("J1");
        var team2 = new Team("J2");

        bool isTeamOneValid = team1.IsValidTeam(unitsPlayer1Names);
        bool isTeamTwoValid = team2.IsValidTeam(unitsPlayer2Names);

        if (!isTeamOneValid || !isTeamTwoValid)
        {
            _gameUi.WriteLine("Archivo de equipos inválido");
            return (team1, team2, false);
        }
        
        return (team1, team2, true);
    }

    private (List<string> unitsPlayer1, List<string> unitsPlayer2) ExtractTeamUnitNames(string[] teamsInfo)
    {
        var unitsPlayer1 = new List<string>();
        var unitsPlayer2 = new List<string>();
        bool isProcessingTeam1 = true;
        
        foreach (var line in teamsInfo[1..])
        {
            if (line == "Player 2 Team")
            {
                isProcessingTeam1 = false;
                continue;
            }
            if (isProcessingTeam1)
                unitsPlayer1.Add(line);
            else
                unitsPlayer2.Add(line);
        }
        
        return (unitsPlayer1, unitsPlayer2);
    }
}