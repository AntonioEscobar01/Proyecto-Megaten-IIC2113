using Shin_Megami_Tensei_View;
namespace Shin_Megami_Tensei;

public class Game
{
    private readonly GameUi _gameUi;
    private readonly TurnManager _turnManager;
    private readonly BattleSystem _battleSystem;
    private readonly TeamLoader _teamLoader;
    private SkillsManager _skillsManager;
    private Team _teamPlayer1;
    private Team _teamPlayer2;
    private bool _isGameOver;
    private int _winnerTeam;

    public Game(View view, string teamsFolderPath)
    {
        _gameUi = new GameUi(view);
        _turnManager = new TurnManager();
        _battleSystem = new BattleSystem(_gameUi);
        _teamLoader = new TeamLoader(_gameUi, teamsFolderPath);
        _skillsManager = new SkillsManager();
        _isGameOver = false;
        _winnerTeam = 0;
    }

    public void Play()
    {
        if (!InitializeTeamsFromFiles())
            return;

        _gameUi.PrintLine();
        _gameUi.PrintPlayerRound(GetCurrentTeam());
        _gameUi.PrintLine();
        RunGameLoop();
        _gameUi.DisplayWinner(_teamPlayer1, _teamPlayer2, _winnerTeam);
    }

    private bool InitializeTeamsFromFiles()
    {
        var loadedTeams = _teamLoader.LoadTeamsFromFile();
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

        _gameUi.DisplayGameState(currentTeam, _teamPlayer1, _teamPlayer2, _turnManager.CurrentTurn);
        
        var enemyTeam = GetEnemyTeam();
        var unitAction = new UnitAction(currentTeam, enemyTeam, _gameUi, _skillsManager, _battleSystem);
        unitAction.ExecuteUnitTurn();

        if (unitAction.ShouldEndGame)
        {
            _isGameOver = true;
            _winnerTeam = (_turnManager.CurrentTurn % 2 != 0) ? 2 : 1;
        }
        
        _gameUi.PrintLine();
        if (!_isGameOver)
        {
            _gameUi.PrintTurnsUsed(currentTeam);
            _gameUi.PrintLine();
        }
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
        }
    }

    private Team GetCurrentTeam()
    {
        return (_turnManager.CurrentTurn % 2 != 0) ? _teamPlayer1 : _teamPlayer2;
    }

    private Team GetEnemyTeam()
    {
        return (_turnManager.CurrentTurn % 2 != 0) ? _teamPlayer2 : _teamPlayer1;
    }
}