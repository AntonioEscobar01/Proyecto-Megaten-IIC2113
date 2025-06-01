namespace Shin_Megami_Tensei;

public class ReviveData
{
    public ReviveParticipants Participants { get; }
    public int MaxHp { get; }
    public int HealAmount { get; }
    
    public ReviveData(ReviveParticipants participants, int maxHp, int healAmount)
    {
        Participants = participants;
        MaxHp = maxHp;
        HealAmount = healAmount;
    }
}