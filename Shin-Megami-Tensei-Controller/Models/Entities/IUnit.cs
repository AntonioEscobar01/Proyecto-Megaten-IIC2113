namespace Shin_Megami_Tensei;

public interface IUnit
{
    string GetName();
    int GetCurrentHp();
    int GetCurrentMp();
    int GetMaxHp();
    int GetMaxMp();
    int GetStr();
    int GetSkl();
    int GetMag();
    int GetSpd();
    int GetLck();
    List<string> GetAbilities();
    SupportEffects GetSupportEffects();
    
    bool IsDead();
    void TakeDamage(int damage);
    void Heal(int amount);
    void ConsumeMp(int amount);
    void ResetStats();
    string GetAffinity(string attackType);
    int GetBaseStat(string statType);
}