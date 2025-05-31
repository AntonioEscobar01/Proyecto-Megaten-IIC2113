namespace Shin_Megami_Tensei;

public class ReviveData
{
    public string TargetName { get; }
    public string ReviverName { get; }
    public int MaxHp { get; }
    public int HealAmount { get; }
    
    public ReviveData(string targetName, string reviverName, int maxHp, int healAmount)
    {
        TargetName = targetName;
        ReviverName = reviverName;
        MaxHp = maxHp;
        HealAmount = healAmount;
    }
}