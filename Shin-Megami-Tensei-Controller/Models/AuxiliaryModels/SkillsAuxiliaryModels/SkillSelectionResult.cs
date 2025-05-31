namespace Shin_Megami_Tensei;

public class SkillSelectionResult
{
    public int SelectedIndex { get; }
    public List<string> Abilities { get; }
    
    public SkillSelectionResult(int selectedIndex, List<string> abilities)
    {
        SelectedIndex = selectedIndex;
        Abilities = abilities;
    }
}