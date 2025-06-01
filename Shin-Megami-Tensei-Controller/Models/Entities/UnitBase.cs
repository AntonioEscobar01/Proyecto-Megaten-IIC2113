namespace Shin_Megami_Tensei;

public abstract class UnitBase : IUnit
{
    private string _name;
    private List<string> _abilities;
    private Affinity _affinities;
    
    private int _originalHp;
    private int _originalMp;
    private int _originalStr;
    private int _originalSkl;
    private int _originalMag;
    private int _originalSpd;
    private int _originalLck;
    
    private int _hp;
    private int _mp;
    private int _str;
    private int _skl;
    private int _mag;
    private int _spd;
    private int _lck;

    protected UnitBase(string name)
    {
        _name = name;
        _abilities = new List<string>();
        _affinities = new Affinity(new Dictionary<string, string>());
    }

    public string GetName() => _name;
    public int GetCurrentHp() => _hp;
    public int GetCurrentMp() => _mp;
    public int GetMaxHp() => _originalHp;
    public int GetMaxMp() => _originalMp;
    public int GetStr() => _str;
    public int GetSkl() => _skl;
    public int GetMag() => _mag;
    public int GetSpd() => _spd;
    public int GetLck() => _lck;
    public List<string> GetAbilities() => new List<string>(_abilities);

    public virtual void ResetStats()
    {
        _hp = _originalHp;
        _mp = _originalMp;
        _str = _originalStr;
        _skl = _originalSkl;
        _mag = _originalMag;
        _spd = _originalSpd;
        _lck = _originalLck;
    }

    public bool IsDead()
    {
        return _hp <= 0;
    }

    public virtual string GetAffinity(string attackType)
    {
        return _affinities.GetAffinity(attackType);
    }

    public virtual void TakeDamage(int damage)
    {
        _hp -= damage;
        if (_hp < 0) 
            _hp = 0;
    }

    public virtual void Heal(int amount)
    {
        _hp += amount;
        if (_hp > _originalHp) 
            _hp = _originalHp;
    }

    public virtual int GetBaseStat(string statType)
    {
        return statType.ToLower() switch
        {
            "str" or "strength" => _str,
            "skl" or "skill" => _skl,
            "mag" or "magic" => _mag,
            "spd" or "speed" => _spd,
            "lck" or "luck" => _lck,
            "hp" => _hp,
            "mp" => _mp,
            _ => 0
        };
    }

    public virtual void ConsumeMp(int amount)
    {
        _mp -= amount;
        if (_mp < 0)
            _mp = 0;
    }
    
    protected abstract void LoadStats();

    protected void SetOriginalHp(int hp) => _originalHp = hp;
    protected void SetOriginalMp(int mp) => _originalMp = mp;
    protected void SetOriginalStr(int str) => _originalStr = str;
    protected void SetOriginalSkl(int skl) => _originalSkl = skl;
    protected void SetOriginalMag(int mag) => _originalMag = mag;
    protected void SetOriginalSpd(int spd) => _originalSpd = spd;
    protected void SetOriginalLck(int lck) => _originalLck = lck;
    
    protected void SetAbilities(List<string> abilities) => _abilities = abilities ?? new List<string>();
    protected void SetAffinities(Affinity affinities) => _affinities = affinities ?? new Affinity(new Dictionary<string, string>());
    
    protected void CopyOriginalStatsToCurrent()
    {
        _hp = _originalHp;
        _mp = _originalMp;
        _str = _originalStr;
        _skl = _originalSkl;
        _mag = _originalMag;
        _spd = _originalSpd;
        _lck = _originalLck;
    }
}