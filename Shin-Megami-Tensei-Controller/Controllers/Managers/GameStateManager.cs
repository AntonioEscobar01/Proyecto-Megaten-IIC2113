namespace Shin_Megami_Tensei;

public class GameStateManager
{
    private readonly Team _teamPlayer1;
    private readonly Team _teamPlayer2;
    private readonly TurnManager _turnManager;
    private readonly GameUiFacade _gameUi;
    private readonly TeamSwitchManager _teamSwitchManager;
    private bool _isGameOver;
    private int _winnerTeam;

    public bool IsGameOver => _isGameOver;
    public int WinnerTeam => _winnerTeam;

    public GameStateManager(Team teamPlayer1, Team teamPlayer2, TurnManager turnManager, 
        GameUiFacade gameUi, TeamSwitchManager teamSwitchManager)
    {
        _teamPlayer1 = teamPlayer1;
        _teamPlayer2 = teamPlayer2;
        _turnManager = turnManager;
        _gameUi = gameUi;
        _teamSwitchManager = teamSwitchManager;
        _isGameOver = false;
        _winnerTeam = 0;
    }

    public void UpdateGameState()
    {
        CheckGameOver();
        HandleTurnTransition(_teamSwitchManager.GetCurrentTeam());
        RemoveDeadUnitsFromBothTeams();
    }

    public void SetGameEndedBySurrender(int winnerTeam)
    {
        _isGameOver = true;
        _winnerTeam = winnerTeam;
    }

    private void CheckGameOver()
    {
        if (_teamPlayer1.AreAllUnitsDead())
        {
            _isGameOver = true;
            _winnerTeam = 2;
        }
        else if (_teamPlayer2.AreAllUnitsDead())
        {
            _isGameOver = true;
            _winnerTeam = 1;
        }
    }

    private void HandleTurnTransition(Team currentTeam)
    {
        if (currentTeam.HasCompletedAllTurns() && !_isGameOver)
        {
            _turnManager.AdvanceTurn();
            _gameUi.PrintPlayerRound(_teamSwitchManager.GetCurrentTeam());
            _gameUi.PrintLine();
            currentTeam.ResetTurns();
            _teamSwitchManager.GetCurrentTeam().InitializeOrderList();
        }
    }

    private void RemoveDeadUnitsFromBothTeams()
    {
        _teamPlayer1.RemoveDeadUnits();
        _teamPlayer2.RemoveDeadUnits();
    }
}