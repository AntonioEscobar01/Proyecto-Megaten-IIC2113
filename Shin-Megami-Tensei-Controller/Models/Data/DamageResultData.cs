namespace Shin_Megami_Tensei;

public class DamageResultData
{
    public int Amount { get; }
    public DamageType Type { get; }
        
    public DamageResultData(int amount, DamageType type)
    {
        Amount = amount;
        Type = type;
    }
}

public enum DamageType
{
    Normal, Resist, Weak, Null, Repel, Drain
}