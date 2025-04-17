namespace Shin_Megami_Tensei;

public class UnitSkillInfo
{
    public string Name { get; }
    public List<string> Abilities { get; }
    public int CurrentMp { get; }
    
    public UnitSkillInfo(string name, List<string> abilities, int currentMp)
    {
        Name = name;
        Abilities = abilities;
        CurrentMp = currentMp;
    }
}