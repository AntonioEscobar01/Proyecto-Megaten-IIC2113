namespace Shin_Megami_Tensei;

public class PositionDisplayInfo
{
    public int Position { get; }
    public UnitDisplayInfo UnitInfo { get; }
    
    public PositionDisplayInfo(int position, UnitDisplayInfo unitInfo)
    {
        Position = position;
        UnitInfo = unitInfo;
    }
}