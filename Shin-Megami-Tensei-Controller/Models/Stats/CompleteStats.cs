namespace Shin_Megami_Tensei;

public class CompleteStats
{
    public CoreStats Core { get; }
    public BattleStats Battle { get; }
    public SecondaryStats Secondary { get; }
    
    public CompleteStats(CoreStats core, BattleStats battle, SecondaryStats secondary)
    {
        Core = core;
        Battle = battle;
        Secondary = secondary;
    }
}