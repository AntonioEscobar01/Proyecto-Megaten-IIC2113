namespace Shin_Megami_Tensei;

public class TurnManager
{
    public int CurrentTurn { get; private set; }

    public TurnManager()
    {
        CurrentTurn = 1;
    }

    public void AdvanceTurn()
    {
        CurrentTurn++;
    }
}