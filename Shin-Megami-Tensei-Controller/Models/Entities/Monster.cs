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
        
        if (monsterData != null)
        {
            SetOriginalStatsFromData(monsterData);
            CopyOriginalStatsToCurrent();
            LoadMonsterAbilities(monsterData);
            
            // Cargar afinidades
            if (monsterData.affinity != null)
            {
                Affinities = new Affinity(monsterData.affinity);
            }
        }
        else
        {
            ApplyDefaultStats();
        }
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