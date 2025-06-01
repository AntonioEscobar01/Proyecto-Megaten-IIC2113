using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class GameUiFacade
{
    private readonly GameUi _gameUi;
    private readonly GameStateDisplayManager _gameStateDisplayManager;
    private readonly ActionMenuManager _actionMenuManager;
    private readonly SelectionMenuManager _selectionMenuManager;
    private readonly CombatMessageManager _combatMessageManager;
    private readonly SkillMessageManager _skillMessageManager;
    private readonly ErrorMessageManager _errorMessageManager;
    private readonly TargetSelectionMessageManager _targetSelectionMessageManager;
    private readonly AllySelectionMessageManager _allySelectionMessageManager;
    private readonly GameUiUtilities _utilities;

    public GameUiFacade(View view)
    {
        _gameUi = new GameUi(view);
        _gameStateDisplayManager = new GameStateDisplayManager(_gameUi);
        _actionMenuManager = new ActionMenuManager(_gameUi);
        _selectionMenuManager = new SelectionMenuManager(_gameUi);
        _combatMessageManager = new CombatMessageManager(_gameUi);
        _skillMessageManager = new SkillMessageManager(_gameUi);
        _errorMessageManager = new ErrorMessageManager(_gameUi);
        _targetSelectionMessageManager = new TargetSelectionMessageManager(_gameUi);
        _allySelectionMessageManager = new AllySelectionMessageManager(_gameUi);
        _utilities = new GameUiUtilities();
    }

    public void PrintLine() => _gameUi.PrintLine();
    public string ReadLine() => _gameUi.ReadLine();
    public void ShowFiles(string[] files) => _gameUi.ShowFiles(files);

    public void DisplayGameState(GameStateDisplayInfo gameStateInfo) => _gameStateDisplayManager.DisplayGameState(gameStateInfo);
    public void PrintPlayerRound(Team currentTeam) => _gameStateDisplayManager.PrintPlayerRound(currentTeam);
    public void DisplayWinner(Team team1, Team team2, int winnerTeam) => _gameStateDisplayManager.DisplayWinner(team1, team2, winnerTeam);

    public int GetSamuraiActionOptions(Samurai samurai) => _actionMenuManager.GetSamuraiActionOptions(samurai);
    public int GetMonsterActionOptions(Monster monster) => _actionMenuManager.GetMonsterActionOptions(monster);

    public int DisplaySummonMenu(List<Monster> availableMonsters) => _selectionMenuManager.DisplaySummonMenu(availableMonsters);
    public int DisplayPositionMenu(Team currentTeam) => _selectionMenuManager.DisplayPositionMenu(currentTeam);

    public void ShowAttack(string attackerName, string actionType, string targetName) => _combatMessageManager.ShowAttack(attackerName, actionType, targetName);
    public void ShowSkillAttack(string attackerName, string actionVerb, string targetName) => _combatMessageManager.ShowSkillAttack(attackerName, actionVerb, targetName);
    public void ShowAffinityResponse(AffinityResponseInfo affinityInfo) => _combatMessageManager.ShowAffinityResponse(affinityInfo);
    public void ShowHpResult(string targetName, int remainingHp, int originalHp) => _combatMessageManager.ShowHpResult(targetName, remainingHp, originalHp);
    public void PrintTurnsUsed(TurnUsageInfo turnUsage) => _combatMessageManager.PrintTurnsUsed(turnUsage);

    public void ShowHealMessage(IUnit target, int healAmount) => _skillMessageManager.ShowHealMessage(target, healAmount);
    public void ShowHealingAction(string healerName, string targetName) => _skillMessageManager.ShowHealingAction(healerName, targetName);
    public void ShowHealAmountReceived(string targetName, int healAmount) => _skillMessageManager.ShowHealAmountReceived(targetName, healAmount);
    public void ShowReviveAction(string reviverName, string targetName) => _skillMessageManager.ShowReviveAction(reviverName, targetName);
    public void ShowSkillSelectionPrompt(string unitName) => _skillMessageManager.ShowSkillSelectionPrompt(unitName);
    public void ShowAffordableSkill(int optionNumber, string skillName, int skillCost) => _skillMessageManager.ShowAffordableSkill(optionNumber, skillName, skillCost);
    public void ShowSkillCancelOption(int optionNumber) => _skillMessageManager.ShowSkillCancelOption(optionNumber);
    public void DisplaySummonSuccess(string monsterName) => _skillMessageManager.DisplaySummonSuccess(monsterName);
    public void ShowSurrenderMessage(string samuraiName, string playerName) => _skillMessageManager.ShowSurrenderMessage(samuraiName, playerName);

    public void ShowInvalidTeamFileError() => _errorMessageManager.ShowInvalidTeamFileError();
    public void ShowFileSelectionPrompt() => _errorMessageManager.ShowFileSelectionPrompt();

    public void ShowTargetSelectionPrompt(string attackerName) => _targetSelectionMessageManager.ShowTargetSelectionPrompt(attackerName);
    public void ShowTargetCancelOption(int targetCount) => _targetSelectionMessageManager.ShowTargetCancelOption(targetCount);
    public void ShowSamuraiTargetOption(int index, UnitDisplayInfo unitInfo) => _targetSelectionMessageManager.ShowSamuraiTargetOption(index, unitInfo);
    public void ShowMonsterTargetOption(int index, UnitDisplayInfo unitInfo) => _targetSelectionMessageManager.ShowMonsterTargetOption(index, unitInfo);
    public void DisplayTargets(List<IUnit> availableTargets) => _targetSelectionMessageManager.DisplayTargets(availableTargets);

    public void ShowAllySelectionPrompt(string healerName) => _allySelectionMessageManager.ShowAllySelectionPrompt(healerName);
    public void ShowAllyCancelOption(int allyCount) => _allySelectionMessageManager.ShowAllyCancelOption(allyCount);
    public void ShowSamuraiAllyOption(int index, UnitDisplayInfo unitInfo) => _allySelectionMessageManager.ShowSamuraiAllyOption(index, unitInfo);
    public void ShowMonsterAllyOption(int index, UnitDisplayInfo unitInfo) => _allySelectionMessageManager.ShowMonsterAllyOption(index, unitInfo);
    public void DisplayAllies(List<IUnit> availableAllies) => _allySelectionMessageManager.DisplayAllies(availableAllies);

    public string GetUnitName(IUnit unit) => _utilities.GetUnitName(unit);
}