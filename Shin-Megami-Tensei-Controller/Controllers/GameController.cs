using Shin_Megami_Tensei_View;
namespace Shin_Megami_Tensei;

public class GameController
{
    private readonly GameUiFacade _gameUi;
    private readonly TurnManager _turnManager;
    private readonly AttackProcessController _attackProcessController;
    private readonly TeamLoadManager _teamLoadManager;
    private SkillsManager _skillsManager;
    private Team? _teamPlayer1;
    private Team? _teamPlayer2;
    private bool _isGameOver;
    private int _winnerTeam;

    public GameController(View view, string teamsFolderPath)
    {
        _gameUi = new GameUiFacade(view);
        _turnManager = new TurnManager();
        _attackProcessController = new AttackProcessController(_gameUi);
        _teamLoadManager = new TeamLoadManager(_gameUi, teamsFolderPath);
        _skillsManager = new SkillsManager();
        _isGameOver = false;
        _winnerTeam = 0;
    }

    public void Play()
    {
        if (!AreTeamsSuccessfullyInitialized())
            return;

        _gameUi.PrintLine();
        _gameUi.PrintPlayerRound(GetCurrentTeam());
        _gameUi.PrintLine();
        RunGameLoop();
        _gameUi.DisplayWinner(_teamPlayer1, _teamPlayer2, _winnerTeam);
    }

    private bool AreTeamsSuccessfullyInitialized()
    {
        var loadedTeams = _teamLoadManager.LoadTeamsFromFile();
        if (!loadedTeams.AreTeamsValid)
            return false;

        _teamPlayer1 = loadedTeams.TeamPlayer1;
        _teamPlayer2 = loadedTeams.TeamPlayer2;
    
        InitializeTeamOrders();
        return true;
    }

    private void InitializeTeamOrders()
    {
        _teamPlayer1.InitializeOrderList();
        _teamPlayer2.InitializeOrderList();
        _teamPlayer1.SetMaxFullTurns();
        _teamPlayer2.SetMaxFullTurns();
    }

    private void RunGameLoop()
    {
        while (!_isGameOver)
        {
            ProcessSingleTurn();
            UpdateGameState();
        }
    }

    private void ProcessSingleTurn()
    {
        var currentTeam = GetCurrentTeam();
        currentTeam.SetMaxFullTurns();

        var teamsInfo = new GameTeamsInfo(_teamPlayer1, _teamPlayer2);
        var gameStateInfo = new GameStateDisplayInfo(currentTeam, teamsInfo, _turnManager.GetCurrentTurn());
        _gameUi.DisplayGameState(gameStateInfo);
    
        var enemyTeam = GetEnemyTeam();
        var unitAction = new UnitActionController(currentTeam, enemyTeam, _gameUi, _skillsManager, _attackProcessController);
        unitAction.ExecuteUnitTurn();

        if (unitAction.ShouldEndGame)
        {
            _isGameOver = true;
            _winnerTeam = (_turnManager.GetCurrentTurn() % 2 != 0) ? 2 : 1;
        }
    
        _gameUi.PrintLine();
    }

    private void UpdateGameState()
    {
        CheckGameOver();
        HandleTurnTransition(GetCurrentTeam());

        _teamPlayer1.RemoveDeadUnits();
        _teamPlayer2.RemoveDeadUnits();
    }

    private void CheckGameOver()
    {
        if (_teamPlayer1.AreAllUnitsDead())
        {
            _isGameOver = true;
            _winnerTeam = 2;
        }
        if (_teamPlayer2.AreAllUnitsDead())
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
            _gameUi.PrintPlayerRound(GetCurrentTeam());
            _gameUi.PrintLine();
            currentTeam.ResetTurns();
            GetCurrentTeam().InitializeOrderList();
        }
    }

    private Team GetCurrentTeam()
    {
        return (_turnManager.GetCurrentTurn() % 2 != 0) ? _teamPlayer1 : _teamPlayer2;
    }

    private Team GetEnemyTeam()
    {
        return (_turnManager.GetCurrentTurn() % 2 != 0) ? _teamPlayer2 : _teamPlayer1;
    }
}