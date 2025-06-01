namespace Shin_Megami_Tensei;

public class GameFlowController
{
    private readonly GameUiFacade _gameUi;
    private readonly TurnManager _turnManager;
    private readonly AttackProcessController _attackProcessController;
    private readonly SkillsManager _skillsManager;
    private readonly GameStateManager _gameStateManager;
    private readonly TeamSwitchManager _teamSwitchManager;
    private readonly Team _teamPlayer1;
    private readonly Team _teamPlayer2;

    public GameFlowController(GameUiFacade gameUi, TurnManager turnManager, 
        AttackProcessController attackProcessController, SkillsManager skillsManager,
        GameStateManager gameStateManager, TeamSwitchManager teamSwitchManager,
        Team teamPlayer1, Team teamPlayer2)
    {
        _gameUi = gameUi;
        _turnManager = turnManager;
        _attackProcessController = attackProcessController;
        _skillsManager = skillsManager;
        _gameStateManager = gameStateManager;
        _teamSwitchManager = teamSwitchManager;
        _teamPlayer1 = teamPlayer1;
        _teamPlayer2 = teamPlayer2;
    }

    public void RunGameLoop()
    {
        while (!_gameStateManager.IsGameOver)
        {
            ProcessSingleTurn();
            _gameStateManager.UpdateGameState();
        }
    }

    private void ProcessSingleTurn()
    {
        var currentTeam = _teamSwitchManager.GetCurrentTeam();
        currentTeam.SetMaxFullTurns();

        var teamsInfo = new GameTeamsInfo(_teamPlayer1, _teamPlayer2);
        var gameStateInfo = new GameStateDisplayInfo(currentTeam, teamsInfo, _turnManager.GetCurrentTurn());
        _gameUi.DisplayGameState(gameStateInfo);

        var enemyTeam = _teamSwitchManager.GetEnemyTeam();
        var unitAction = new UnitActionController(currentTeam, enemyTeam, _gameUi, _skillsManager, _attackProcessController);
        unitAction.ExecuteUnitTurn();

        if (unitAction.ShouldEndGame)
        {
            int winnerTeam = (_turnManager.GetCurrentTurn() % 2 != 0) ? 2 : 1;
            _gameStateManager.SetGameEndedBySurrender(winnerTeam);
        }

        _gameUi.PrintLine();
    }
}