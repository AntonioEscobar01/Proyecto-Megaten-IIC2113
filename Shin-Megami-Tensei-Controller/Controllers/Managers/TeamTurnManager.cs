namespace Shin_Megami_Tensei;

public class TeamTurnManager
{
    private int _fullTurns;
    private int _blinkingTurns;
    private int _maxFullTurns;
    private int _initialMaxFullTurns;
    private int _usedSkillsCount;

    public TeamTurnManager()
    {
        _fullTurns = 0;
        _blinkingTurns = 0;
        _usedSkillsCount = 0;
    }

    public int GetFullTurns() => _fullTurns;
    public int GetBlinkingTurns() => _blinkingTurns;
    public int GetMaxFullTurns() => _maxFullTurns;
    public int GetUsedSkillsCount() => _usedSkillsCount;

    public void SetMaxFullTurns(int orderListCount)
    {
        if (IsInitialTurnSetup())
        {
            _maxFullTurns = orderListCount;
            _initialMaxFullTurns = _maxFullTurns;
        }
        else 
        {
            _maxFullTurns = _initialMaxFullTurns;
        }
    }

    public void ResetTurns()
    {
        _fullTurns = 0;
    }

    public bool HasCompletedAllTurns()
    {
        return HasCompletedAllFullTurns() && HasNoBlinkingTurns();
    }

    public void ConsumeFullTurn()
    {
        _fullTurns++;
    }

    public void ConsumeBlinkingTurn()
    {
        _blinkingTurns--;
    }

    public void AddBlinkingTurn()
    {
        _blinkingTurns++;
    }

    public void IncrementUsedSkillsCount()
    {
        _usedSkillsCount++;
    }

    public void ConsumeSummonTurns()
    {
        if (HasBlinkingTurns())
            ConsumeBlinkingTurn();
        else
        {
            ConsumeFullTurn();
            AddBlinkingTurn();
        }
    }
    
    public void ConsumeNonOffensiveSkillsTurns()
    {
        if (HasBlinkingTurns())
            ConsumeBlinkingTurn();
        else
        {
            ConsumeFullTurn();
        }
    }

    private bool IsInitialTurnSetup()
    {
        return _fullTurns == 0;
    }

    private bool HasCompletedAllFullTurns()
    {
        return _fullTurns == _maxFullTurns;
    }

    private bool HasNoBlinkingTurns()
    {
        return _blinkingTurns == 0;
    }

    private bool HasBlinkingTurns()
    {
        return _blinkingTurns > 0;
    }
}