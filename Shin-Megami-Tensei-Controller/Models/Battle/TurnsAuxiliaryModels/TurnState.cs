namespace Shin_Megami_Tensei;

public class TurnState
{
    public int FullTurns { get; }
    public int BlinkingTurns { get; }
    
    public TurnState(int fullTurns, int blinkingTurns)
    {
        FullTurns = fullTurns;
        BlinkingTurns = blinkingTurns;
    }
}