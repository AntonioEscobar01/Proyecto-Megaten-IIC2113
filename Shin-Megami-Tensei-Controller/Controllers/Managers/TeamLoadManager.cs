using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class TeamLoadManager
{
    private readonly GameUiFacade _gameUi;
    private readonly string _teamsFolder;

    public TeamLoadManager(GameUiFacade gameUi, string teamsFolder)
    {
        _gameUi = gameUi;
        _teamsFolder = teamsFolder;
    }

    public (Team TeamPlayer1, Team TeamPlayer2, bool AreTeamsValid) LoadTeamsFromFile()
    {
        var teamFiles = Directory.GetFiles(_teamsFolder);
        string chosenFile = GetUserSelectedFile(teamFiles);
        var teamsInfo = File.ReadAllLines(chosenFile);

        var (unitsPlayer1Names, unitsPlayer2Names) = ExtractTeamUnitNames(teamsInfo);

        var team1 = new Team("J1");
        var team2 = new Team("J2");

        bool areTeamsValid = ValidateTeams(team1, team2, unitsPlayer1Names, unitsPlayer2Names);

        if (!areTeamsValid)
        {
            _gameUi.WriteLine("Archivo de equipos inválido");
        }
        
        return (team1, team2, areTeamsValid);
    }

    private string GetUserSelectedFile(string[] teamFiles)
    {
        _gameUi.WriteLine("Elige un archivo para cargar los equipos");
        _gameUi.ShowFiles(teamFiles);
        
        int chosenFileIndex = int.Parse(_gameUi.ReadLine());
        return teamFiles[chosenFileIndex];
    }

    private bool ValidateTeams(Team team1, Team team2, List<string> unitsPlayer1Names, List<string> unitsPlayer2Names)
    {
        bool isTeamOneValid = team1.IsValidTeam(unitsPlayer1Names);
        bool isTeamTwoValid = team2.IsValidTeam(unitsPlayer2Names);
        
        return isTeamOneValid && isTeamTwoValid;
    }

    private (List<string> unitsPlayer1, List<string> unitsPlayer2) ExtractTeamUnitNames(string[] teamsInfo)
    {
        var unitsPlayer1 = new List<string>();
        var unitsPlayer2 = new List<string>();
        bool isProcessingTeam1 = true;
        
        ProcessTeamLines(teamsInfo, unitsPlayer1, unitsPlayer2, ref isProcessingTeam1);
        
        return (unitsPlayer1, unitsPlayer2);
    }

    private void ProcessTeamLines(string[] teamsInfo, List<string> unitsPlayer1, List<string> unitsPlayer2, ref bool isProcessingTeam1)
    {
        foreach (var line in teamsInfo[1..])
        {
            if (IsPlayer2TeamHeader(line))
            {
                isProcessingTeam1 = false;
                continue;
            }
            
            AddUnitToAppropriateTeam(line, unitsPlayer1, unitsPlayer2, isProcessingTeam1);
        }
    }

    private bool IsPlayer2TeamHeader(string line)
    {
        return line == "Player 2 Team";
    }

    private void AddUnitToAppropriateTeam(string line, List<string> unitsPlayer1, List<string> unitsPlayer2, bool isProcessingTeam1)
    {
        if (isProcessingTeam1)
            unitsPlayer1.Add(line);
        else
            unitsPlayer2.Add(line);
    }
}