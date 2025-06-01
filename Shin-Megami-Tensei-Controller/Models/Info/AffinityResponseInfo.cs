namespace Shin_Megami_Tensei;

public class AffinityResponseInfo
{
    public CombatNames Names { get; }
    public AffinityResponseData Data { get; }
    
    public string AttackerName => Names.AttackerName;
    public string TargetName => Names.TargetName;
    public string Affinity => Data.Affinity;
    public int Damage => Data.Damage;
    
    public AffinityResponseInfo(CombatNames names, AffinityResponseData data)
    {
        Names = names;
        Data = data;
    }
}