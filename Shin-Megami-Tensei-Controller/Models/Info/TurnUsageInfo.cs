namespace Shin_Megami_Tensei;

public class TurnUsageInfo
{
    public int FullTurnsUsed { get; }
    public int BlinkingTurnsUsed { get; }
    public int BlinkingTurnsGained { get; }
    
    public TurnUsageInfo(int fullTurnsUsed, int blinkingTurnsUsed, int blinkingTurnsGained)
    {
        FullTurnsUsed = fullTurnsUsed;
        BlinkingTurnsUsed = blinkingTurnsUsed;
        BlinkingTurnsGained = blinkingTurnsGained;
    }
}