namespace Shin_Megami_Tensei;

public class CombatNames
{
    public string AttackerName { get; }
    public string TargetName { get; }
    
    public CombatNames(string attackerName, string targetName)
    {
        AttackerName = attackerName;
        TargetName = targetName;
    }
}