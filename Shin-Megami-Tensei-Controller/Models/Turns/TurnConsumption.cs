namespace Shin_Megami_Tensei;

public class TurnConsumption
{
    public int FullTurnsUsed { get; }
    public int BlinkingTurnsUsed { get; }
    public int BlinkingTurnsGained { get; }
    
    public TurnConsumption(int fullTurnsUsed, int blinkingTurnsUsed, int blinkingTurnsGained)
    {
        FullTurnsUsed = fullTurnsUsed;
        BlinkingTurnsUsed = blinkingTurnsUsed;
        BlinkingTurnsGained = blinkingTurnsGained;
    }
}