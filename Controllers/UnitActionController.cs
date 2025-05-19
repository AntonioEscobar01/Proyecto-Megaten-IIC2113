// ...existing code...

public bool ProcessAction(UnitBase unit, UnitActionType action, Team playerTeam, Team enemyTeam)
{
    if (IsUnitDead(unit))
    {
        return false;
    }

    bool actionResult = false;
    
    switch (action)
    {
        case UnitActionType.Attack:
            actionResult = CanProcessAttackAction(unit, enemyTeam);
            break;
        case UnitActionType.Skill:
            actionResult = CanProcessSkillAction(unit, playerTeam, enemyTeam);
            break;
        case UnitActionType.Invoke:
            actionResult = CanProcessInvokeAction(unit, playerTeam, enemyTeam);
            break;
        case UnitActionType.Pass:
            actionResult = ProcessPassAction(unit);
            break;
        case UnitActionType.Surrender:
            actionResult = ProcessSurrenderAction(unit);
            break;
    }
    
    return actionResult;
}

private bool IsUnitDead(UnitBase unit)
{
    if (unit.IsDead)
    {
        _gameUi.WriteLine($"{unit.Name} está muerto y no puede actuar.");
        return true;
    }
    return false;
}

public bool CanProcessAttackAction(UnitBase attacker, Team enemyTeam)
{
    UnitBase target = SelectTarget(enemyTeam);
    if (target == null)
    {
        return false;
    }
    
    return ExecuteAttack(attacker, target);
}

private UnitBase SelectTarget(Team enemyTeam)
{
    return _targetSelectorController.ChooseUnitToAttack(enemyTeam);
}

private bool ExecuteAttack(UnitBase attacker, UnitBase target)
{
    int damage = CalculateBaseDamage(attacker);
    bool isCritical = DetermineIfCritical();
    
    _gameUi.WriteLine($"{attacker.Name} ataca a {target.Name}.");
    bool targetDied = _combatProcessor.CanProcessAttackWithAffinity(attacker, target, damage, 1, isCritical, SkillElement.Neutral, null);
    
    if (targetDied)
    {
        _gameUi.WriteLine($"{target.Name} ha sido derrotado.");
    }
    
    return true;
}

private int CalculateBaseDamage(UnitBase attacker)
{
    return attacker is Samurai ? ((Samurai)attacker).Strength : ((Monster)attacker).Strength;
}

private bool DetermineIfCritical()
{
    Random random = new Random();
    return random.Next(100) < 10;
}

public bool CanProcessSkillAction(UnitBase caster, Team playerTeam, Team enemyTeam)
{
    SkillData selectedSkill = GetSelectedSkill(caster);
    if (selectedSkill == null)
    {
        return false;
    }
    
    return ExecuteSkill(caster, selectedSkill, playerTeam, enemyTeam);
}

private SkillData GetSelectedSkill(UnitBase caster)
{
    List<SkillData> affordableSkills = GetAffordableSkills(caster);
    return GetSkillFromSelection(affordableSkills);
}

private List<SkillData> GetAffordableSkills(UnitBase caster)
{
    List<SkillData> affordableSkills = new List<SkillData>();
    
    _gameUi.WriteLine("Habilidades disponibles:");
    
    foreach (SkillData skill in caster.Skills)
    {
        if (caster.SkillTurns >= skill.TurnCost)
        {
            affordableSkills.Add(skill);
        }
    }
    
    return affordableSkills;
}

private SkillData GetSkillFromSelection(List<SkillData> affordableSkills)
{
    if (affordableSkills.Count == 0)
    {
        _gameUi.WriteLine("No hay habilidades disponibles.");
        return null;
    }
    
    _gameUi.WriteLine("Selecciona una habilidad:");
    
    for (int i = 0; i < affordableSkills.Count; i++)
    {
        _gameUi.WriteLine($"{i + 1}. {affordableSkills[i].Name} (Costo: {affordableSkills[i].TurnCost} turnos)");
    }
    
    int selection = _input.GetIntInput(1, affordableSkills.Count) - 1;
    return affordableSkills[selection];
}

// Continuar con la refactorización de los demás métodos...

public void HealTarget(UnitBase healer, UnitBase target, int healAmount)
{
    if (target.IsDead)
    {
        _gameUi.WriteLine($"{target.Name} está muerto y no puede ser curado.");
        return;
    }
    
    ApplyHealing(target, healAmount);
}

private void ApplyHealing(UnitBase target, int healAmount)
{
    int actualHeal = Math.Min(healAmount, target.MaxHp - target.CurrentHp);
    target.CurrentHp += actualHeal;
    
    _gameUi.WriteLine($"{target.Name} ha sido curado por {actualHeal} puntos de vida.");
}

// Eliminar código duplicado en ConsumeAllTurns y ConsumeMultipleBlinkingTurns
private void ConsumeBlinkingTurns(UnitBase unit, int turnsToConsume)
{
    unit.BlinkingTurns = Math.Max(0, unit.BlinkingTurns - turnsToConsume);
}

public void ConsumeAllTurns(UnitBase unit)
{
    unit.SkillTurns = 0;
    ConsumeBlinkingTurns(unit, unit.BlinkingTurns);
    unit.IsCharging = false;
}

public void ConsumeMultipleBlinkingTurns(UnitBase unit, int turnsToConsume)
{
    ConsumeBlinkingTurns(unit, turnsToConsume);
    if (unit.BlinkingTurns == 0)
    {
        _gameUi.WriteLine($"{unit.Name} ha dejado de parpadear.");
    }
}

// ...existing code...
