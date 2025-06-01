namespace Shin_Megami_Tensei;

public class ReviveParticipants
{
    public string TargetName { get; }
    public string ReviverName { get; }
    
    public ReviveParticipants(string targetName, string reviverName)
    {
        TargetName = targetName;
        ReviverName = reviverName;
    }
}