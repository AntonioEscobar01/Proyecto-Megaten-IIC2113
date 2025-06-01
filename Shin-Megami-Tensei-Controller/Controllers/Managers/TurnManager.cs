namespace Shin_Megami_Tensei;

public class TurnManager
{
    private int _currentTurn;

    public TurnManager()
    {
        _currentTurn = 1;
    }

    public int GetCurrentTurn() => _currentTurn;

    public void AdvanceTurn()
    {
        _currentTurn++;
    }
}