namespace Shin_Megami_Tensei;

public class HealthInfo
{
    public int Current { get; }
    public int Max { get; }
    
    public HealthInfo(int current, int max)
    {
        Current = current;
        Max = max;
    }
}