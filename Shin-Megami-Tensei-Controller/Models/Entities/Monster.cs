namespace Shin_Megami_Tensei;

public class Monster : UnitBase
{
    private static MonsterStatsManager _statsRepository;
    private const int DefaultHp = 100;
    private const int DefaultMp = 50;
    private const int DefaultStat = 10;

    static Monster()
    {
        _statsRepository = new MonsterStatsManager();
    }

    public Monster(string name) : base(name)
    {
        InitializeMonsterStats();
    }

    private void InitializeMonsterStats()
    {
        LoadStats();
    }

    protected override void LoadStats()
    {
        var monsterData = _statsRepository.GetMonsterData(Name);
    
        if (HasValidMonsterData(monsterData))
        {
            LoadStatsFromData(monsterData);
        }
        else
        {
            ApplyDefaultStats();
        }
    }
    
    private void LoadStatsFromData(MonsterData monsterData)
    {
        SetOriginalStatsFromData(monsterData);
        CopyOriginalStatsToCurrent();
        LoadMonsterAbilities(monsterData);
        LoadMonsterAffinities(monsterData);
    }
    

    private void LoadMonsterAffinities(MonsterData monsterData)
    {
        if (HasValidAffinities(monsterData))
        {
            Affinities = new Affinity(monsterData.affinity);
        }
    }

    private bool HasValidMonsterData(MonsterData monsterData)
    {
        return monsterData != null;
    }

    private bool HasValidSkills(MonsterData monsterData)
    {
        return monsterData.skills != null;
    }

    private bool HasValidAffinities(MonsterData monsterData)
    {
        return monsterData.affinity != null;
    }

    private void SetOriginalStatsFromData(MonsterData monsterData)
    {
        OriginalHp = monsterData.stats.HP;
        OriginalMp = monsterData.stats.MP;
        OriginalStr = monsterData.stats.Str;
        OriginalSkl = monsterData.stats.Skl;
        OriginalMag = monsterData.stats.Mag;
        OriginalSpd = monsterData.stats.Spd;
        OriginalLck = monsterData.stats.Lck;
    }

    private void LoadMonsterAbilities(MonsterData monsterData)
    {
        if (monsterData.skills != null)
        {
            Abilities = monsterData.skills;
        }
    }

    private void ApplyDefaultStats()
    {
        OriginalHp = DefaultHp;
        OriginalMp = DefaultMp;
        OriginalStr = DefaultStat;
        OriginalSkl = DefaultStat;
        OriginalMag = DefaultStat;
        OriginalSpd = DefaultStat;
        OriginalLck = DefaultStat;
        CopyOriginalStatsToCurrent();
    }
}