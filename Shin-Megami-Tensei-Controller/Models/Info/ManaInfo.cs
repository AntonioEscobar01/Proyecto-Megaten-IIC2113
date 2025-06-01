namespace Shin_Megami_Tensei;

public class ManaInfo
{
    public int Current { get; }
    public int Max { get; }
    
    public ManaInfo(int current, int max)
    {
        Current = current;
        Max = max;
    }
}