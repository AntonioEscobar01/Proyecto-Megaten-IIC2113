namespace Shin_Megami_Tensei;

public abstract class UnitBase : IUnit
{
    public string Name { get; protected set; }
    public List<string> Abilities { get; protected set; }
    public Affinity Affinities { get; protected set; }
    
    public int OriginalHp { get; protected set; }
    public int OriginalMp { get; protected set; }
    public int OriginalStr { get; protected set; }
    public int OriginalSkl { get; protected set; }
    public int OriginalMag { get; protected set; }
    public int OriginalSpd { get; protected set; }
    public int OriginalLck { get; protected set; }
    
    public int Hp { get; set; }
    public int Mp { get; set; }
    public int Str { get; set; }
    public int Skl { get; set; }
    public int Mag { get; set; }
    public int Spd { get; set; }
    public int Lck { get; set; }

    protected UnitBase(string name)
    {
        Name = name;
        Abilities = new List<string>();
        Affinities = new Affinity(new Dictionary<string, string>());
    }

    public virtual void ResetStats()
    {
        Hp = OriginalHp;
        Mp = OriginalMp;
        Str = OriginalStr;
        Skl = OriginalSkl;
        Mag = OriginalMag;
        Spd = OriginalSpd;
        Lck = OriginalLck;
    }

    public bool IsDead()
    {
        return IsHealthAtZero();
    }
    
    private bool IsHealthAtZero()
    {
        return Hp <= 0;
    }

    public virtual string GetAffinity(string attackType)
    {
        return Affinities.GetAffinity(attackType);
    }

    public virtual void TakeDamage(int damage)
    {
        Hp -= damage;
        if (Hp < 0) 
            Hp = 0;
    }

    public virtual void Heal(int amount)
    {
        Hp += amount;
        if (Hp > OriginalHp) 
            Hp = OriginalHp;
    }

    public virtual int GetBaseStat(string statType)
    {
        return statType.ToLower() switch
        {
            "str" or "strength" => Str,
            "skl" or "skill" => Skl,
            "mag" or "magic" => Mag,
            "spd" or "speed" => Spd,
            "lck" or "luck" => Lck,
            "hp" => Hp,
            "mp" => Mp,
            _ => 0
        };
    }

    public virtual void ConsumeMp(int amount)
    {
        Mp -= amount;
        if (Mp < 0)
            Mp = 0;
    }
    
    protected abstract void LoadStats();
    
    protected void CopyOriginalStatsToCurrent()
    {
        Hp = OriginalHp;
        Mp = OriginalMp;
        Str = OriginalStr;
        Skl = OriginalSkl;
        Mag = OriginalMag;
        Spd = OriginalSpd;
        Lck = OriginalLck;
    }
}