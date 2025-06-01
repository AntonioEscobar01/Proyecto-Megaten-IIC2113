namespace Shin_Megami_Tensei;

public class CombatMessageManager
{
    private readonly GameUi _gameUi;

    public CombatMessageManager(GameUi gameUi)
    {
        _gameUi = gameUi;
    }

    public void ShowAttack(string attackerName, string actionType, string targetName)
    {
        _gameUi.WriteLine($"{attackerName} {actionType} a {targetName}");
    }

    public void ShowAffinityResponse(AffinityResponseInfo affinityInfo)
    {
        switch (affinityInfo.Affinity)
        {
            case "Rs":
                ShowResistResponse(affinityInfo.AttackerName, affinityInfo.TargetName, affinityInfo.Damage);
                break;
            case "Wk":
                ShowWeakResponse(affinityInfo.AttackerName, affinityInfo.TargetName, affinityInfo.Damage);
                break;
            case "Nu":
                ShowNullResponse(affinityInfo.AttackerName, affinityInfo.TargetName);
                break;
            case "Dr":
                ShowDrainResponse(affinityInfo.TargetName, affinityInfo.Damage);
                break;
            case "Rp":
                ShowRepelResponse(affinityInfo.AttackerName, affinityInfo.TargetName, affinityInfo.Damage);
                break;
            default:
                ShowNormalDamageResponse(affinityInfo.TargetName, affinityInfo.Damage);
                break;
        }
    }

    public void ShowHpResult(string targetName, int remainingHp, int originalHp)
    {
        _gameUi.WriteLine($"{targetName} termina con HP:{remainingHp}/{originalHp}");
    }

    public void PrintTurnsUsed(TurnUsageInfo turnUsage)
    {
        _gameUi.PrintLine();
        _gameUi.WriteLine($"Se han consumido {turnUsage.FullTurnsUsed} Full Turn(s) y {turnUsage.BlinkingTurnsUsed} Blinking Turn(s)");
        _gameUi.WriteLine($"Se han obtenido {turnUsage.BlinkingTurnsGained} Blinking Turn(s)");
    }

    private void ShowResistResponse(string attackerName, string targetName, int damage)
    {
        _gameUi.WriteLine($"{targetName} es resistente el ataque de {attackerName}");
        ShowDamageReceived(targetName, damage);
    }

    private void ShowWeakResponse(string attackerName, string targetName, int damage)
    {
        _gameUi.WriteLine($"{targetName} es débil contra el ataque de {attackerName}");
        ShowDamageReceived(targetName, damage);
    }

    private void ShowNullResponse(string attackerName, string targetName)
    {
        _gameUi.WriteLine($"{targetName} bloquea el ataque de {attackerName}");
    }

    private void ShowDrainResponse(string targetName, int damage)
    {
        _gameUi.WriteLine($"{targetName} absorbe {damage} daño");
    }

    private void ShowRepelResponse(string attackerName, string targetName, int damage)
    {
        _gameUi.WriteLine($"{targetName} devuelve {damage} daño a {attackerName}");
    }

    private void ShowNormalDamageResponse(string targetName, int damage)
    {
        ShowDamageReceived(targetName, damage);
    }

    private void ShowDamageReceived(string targetName, int damage)
    {
        _gameUi.WriteLine($"{targetName} recibe {damage} de daño");
    }
}