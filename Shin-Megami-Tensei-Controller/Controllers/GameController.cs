using Shin_Megami_Tensei_View;
namespace Shin_Megami_Tensei;

public class GameController
{
    private readonly GameUiFacade _gameUi;
    private readonly GameInitializer _gameInitializer;
    private GameFlowController _gameFlowController;
    private GameStateManager _gameStateManager;
    private TeamSwitchManager _teamSwitchManager;
    private Team _teamPlayer1;
    private Team _teamPlayer2;

    public GameController(View view, string teamsFolderPath)
    {
        _gameUi = new GameUiFacade(view);
        var teamLoadManager = new TeamLoadManager(_gameUi, teamsFolderPath);
        _gameInitializer = new GameInitializer(teamLoadManager);
    }

    public void Play()
    {
        if (!InitializeGame())
            return;

        _gameUi.PrintLine();
        _gameUi.PrintPlayerRound(_teamSwitchManager.GetCurrentTeam());
        _gameUi.PrintLine();
        
        _gameFlowController.RunGameLoop();
        _gameUi.DisplayWinner(_teamPlayer1, _teamPlayer2, _gameStateManager.WinnerTeam);
    }

    private bool InitializeGame()
    {
        var initializationResult = _gameInitializer.InitializeTeams();
        if (!initializationResult.IsSuccessful)
            return false;

        _teamPlayer1 = initializationResult.TeamPlayer1;
        _teamPlayer2 = initializationResult.TeamPlayer2;

        SetupGameComponents();
        return true;
    }

    private void SetupGameComponents()
    {
        var turnManager = new TurnManager();
        var attackProcessController = new AttackProcessController(_gameUi);
        var skillsManager = new SkillsManager();
        
        _teamSwitchManager = new TeamSwitchManager(_teamPlayer1, _teamPlayer2, turnManager);
        _gameStateManager = new GameStateManager(_teamPlayer1, _teamPlayer2, turnManager, _gameUi, _teamSwitchManager);
        
        _gameFlowController = new GameFlowController(_gameUi, turnManager, attackProcessController, 
            skillsManager, _gameStateManager, _teamSwitchManager, _teamPlayer1, _teamPlayer2);
    }
}