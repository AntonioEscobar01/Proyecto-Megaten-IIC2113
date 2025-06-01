namespace Shin_Megami_Tensei;

public class MonsterPlacement
{
    public Monster SelectedMonster { get; }
    public int SelectedPosition { get; }
    
    public MonsterPlacement(Monster selectedMonster, int selectedPosition)
    {
        SelectedMonster = selectedMonster;
        SelectedPosition = selectedPosition;
    }
}