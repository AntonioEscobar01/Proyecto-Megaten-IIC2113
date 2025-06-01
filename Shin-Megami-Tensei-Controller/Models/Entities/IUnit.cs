namespace Shin_Megami_Tensei;

public interface IUnit
{
    string Name { get; }
    int Hp { get; set; }
    int Mp { get; set; }
    int OriginalHp { get; }
    int OriginalMp { get; }
    int Str { get; }
    int Skl { get; }
    int Mag { get; }
    int Spd { get; }
    int Lck { get; }
    List<string> Abilities { get; }
    Affinity Affinities { get; }
    
    bool IsDead();
    void ResetStats();
    string GetAffinity(string attackType);
    void TakeDamage(int damage);
    void Heal(int amount);
    int GetBaseStat(string statType);
    void ConsumeMp(int amount);
}