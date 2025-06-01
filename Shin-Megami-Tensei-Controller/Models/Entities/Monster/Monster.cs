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
        var monsterData = _statsRepository.GetMonsterData(GetName());
    
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
            SetAffinities(new Affinity(monsterData.affinity));
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
        SetOriginalHp(monsterData.stats.HP);
        SetOriginalMp(monsterData.stats.MP);
        SetOriginalStr(monsterData.stats.Str);
        SetOriginalSkl(monsterData.stats.Skl);
        SetOriginalMag(monsterData.stats.Mag);
        SetOriginalSpd(monsterData.stats.Spd);
        SetOriginalLck(monsterData.stats.Lck);
    }

    private void LoadMonsterAbilities(MonsterData monsterData)
    {
        if (monsterData.skills != null)
        {
            SetAbilities(monsterData.skills);
        }
    }

    private void ApplyDefaultStats()
    {
        SetOriginalHp(DefaultHp);
        SetOriginalMp(DefaultMp);
        SetOriginalStr(DefaultStat);
        SetOriginalSkl(DefaultStat);
        SetOriginalMag(DefaultStat);
        SetOriginalSpd(DefaultStat);
        SetOriginalLck(DefaultStat);
        CopyOriginalStatsToCurrent();
    }
}