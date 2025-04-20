using System.Text.Json;
namespace Shin_Megami_Tensei;

public class Monster
{
    public string Name { get; private set; }
    public List<string> Abilities { get; private set; }
    public Affinity Affinities { get; private set; }

    // Original stats (immutable)
    public int OriginalHp { get; private set; }
    public int OriginalMp { get; private set; }
    public int OriginalStr { get; private set; }
    public int OriginalSkl { get; private set; }
    public int OriginalMag { get; private set; }
    public int OriginalSpd { get; private set; }
    public int OriginalLck { get; private set; }

    // Current stats (mutable)
    public int Hp { get; set; }
    public int Mp { get; set; }
    public int Str { get; set; }
    public int Skl { get; set; }
    public int Mag { get; set; }
    public int Spd { get; set; }
    public int Lck { get; set; }

    private static MonsterStatsManager _statsRepository;
    private const int DefaultHp = 100;
    private const int DefaultMp = 50;
    private const int DefaultStat = 10;

    static Monster()
    {
        _statsRepository = new MonsterStatsManager();
    }

    public Monster(string name)
    {
        Name = name;
        Abilities = new List<string>();
        InitializeMonsterStats();
    }

    private void InitializeMonsterStats()
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
            else
            {
                Affinities = new Affinity(new Dictionary<string, string>());
            }
        }
        else
        {
            ApplyDefaultStats();
            Affinities = new Affinity(new Dictionary<string, string>());
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

    private void CopyOriginalStatsToCurrent()
    {
        Hp = OriginalHp;
        Mp = OriginalMp;
        Str = OriginalStr;
        Skl = OriginalSkl;
        Mag = OriginalMag;
        Spd = OriginalSpd;
        Lck = OriginalLck;
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
        OriginalHp = Hp = DefaultHp;
        OriginalMp = Mp = DefaultMp;
        OriginalStr = Str = DefaultStat;
        OriginalSkl = Skl = DefaultStat;
        OriginalMag = Mag = DefaultStat;
        OriginalSpd = Spd = DefaultStat;
        OriginalLck = Lck = DefaultStat;
    }

    public void ResetStats()
    {
        CopyOriginalStatsToCurrent();
    }
    
    public bool IsDead()
    {
        return Hp <= 0;
    }
}