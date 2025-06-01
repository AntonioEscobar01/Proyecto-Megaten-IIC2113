namespace Shin_Megami_Tensei;

public class UnitDisplayInfo
{
    public string Name { get; }
    public HealthInfo Health { get; }
    public ManaInfo Mana { get; }
    
    public UnitDisplayInfo(string name, HealthInfo health, ManaInfo mana)
    {
        Name = name;
        Health = health;
        Mana = mana;
    }
}