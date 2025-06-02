namespace Shin_Megami_Tensei;

public class AttackProcessController
{
    private readonly GameUiFacade _gameUi;
    private readonly DamageCalculator _damageCalculator;
    private readonly AffinityProcessorController _affinityProcessorController;
    private readonly DamageApplicator _damageApplicator;
    
    private const string PHYS_TYPE = "Phys";
    private const string GUN_TYPE = "Gun";

    public AttackProcessController(GameUiFacade gameUi)
    {
        _gameUi = gameUi;
        _damageCalculator = new DamageCalculator();
        _affinityProcessorController = new AffinityProcessorController();
        _damageApplicator = new DamageApplicator(gameUi);
    }

    public void Attack(IUnit attacker, IUnit target)
    {
        double baseDamage = _damageCalculator.CalculateBasicAttackDamage(attacker);
        var participants = new CombatParticipants(attacker, target);
        var attackInfo = new AttackInfo(baseDamage, PHYS_TYPE, "ataca");
        var attackContext = new AttackContext(participants, attackInfo);
        ProcessAttackWithAffinity(attackContext);
    }

    public void Shoot(IUnit attacker, IUnit target)
    {
        double baseDamage = _damageCalculator.CalculateShootDamage(attacker);
        var participants = new CombatParticipants(attacker, target);
        var attackInfo = new AttackInfo(baseDamage, GUN_TYPE, "dispara");
        var attackContext = new AttackContext(participants, attackInfo);
        ProcessAttackWithAffinity(attackContext);
    }
    
    public string ApplyOffensiveSkill(IUnit attacker, IUnit target, OffensiveSkillInfo skillInfo)
    {
        string affinity = target.GetAffinity(skillInfo.Skill.type);
        int numberOfHits = skillInfo.Skill.GetHitsCount(skillInfo.UsedSkillsCount);
        
        var participants = new CombatParticipants(attacker, target);
        
        if (IsInstantKillSkill(skillInfo.Skill.type))
        {
            var executionInfo = new SkillExecutionInfo(skillInfo.Skill, affinity, 0);
            var skillContext = new SkillExecutionContext(participants, executionInfo, numberOfHits);
            ExecuteInstantKillSkillHits(skillContext);
        }
        else
        {
            double baseDamage = _damageCalculator.CalculateSkillDamage(attacker, skillInfo.Skill);
            var executionInfo = new SkillExecutionInfo(skillInfo.Skill, affinity, baseDamage);
            var skillContext = new SkillExecutionContext(participants, executionInfo, numberOfHits);
            ExecuteSkillHits(skillContext);
        }
        
        ShowFinalSkillResult(attacker, target, affinity);
        return affinity;
    }

    private void ProcessAttackWithAffinity(AttackContext context)
    {
        string attackerName = context.Attacker.GetName();
        string targetName = context.Target.GetName();
        string affinity = context.Target.GetAffinity(context.AttackType);
    
        _gameUi.ShowAttack(attackerName, context.ActionType, targetName);
    
        var damageResult = _affinityProcessorController.ProcessAffinityDamage(context.BaseDamage, affinity);
        var damageInfo = new DamageContextInfo(damageResult, affinity);
        var damageContext = new DamageApplicationContext(context.Participants, damageInfo);
        _damageApplicator.ApplyDamageBasedOnAffinity(damageContext);
    
        var resultContext = new AttackResultContext(context.Participants, affinity);
        ShowDamageResultForAttack(resultContext);
    }

    private void ExecuteSkillHits(SkillExecutionContext context)
    {
        for (int i = 0; i < context.NumberOfHits; i++)
        {
            var hitContext = new SingleHitContext(context.Participants, context.Info);
            ExecuteSingleSkillHit(hitContext);
        }
    }

    private void ExecuteInstantKillSkillHits(SkillExecutionContext context)
    {
        for (int i = 0; i < context.NumberOfHits; i++)
        {
            var hitContext = new SingleHitContext(context.Participants, context.Info);
            ExecuteSingleInstantKillHit(hitContext);
        }
    }

    private void ExecuteSingleSkillHit(SingleHitContext hitContext)
    {
        ShowSkillAttackMessage(hitContext.Attacker, hitContext.Target, hitContext.Skill);
        var damageResult = _affinityProcessorController.ProcessAffinityDamage(hitContext.BaseDamage, hitContext.Affinity);
        var damageInfo = new DamageContextInfo(damageResult, hitContext.Affinity);
        var participants = new CombatParticipants(hitContext.Attacker, hitContext.Target);
        var damageContext = new DamageApplicationContext(participants, damageInfo);
        _damageApplicator.ApplyDamageBasedOnAffinity(damageContext);
    }

    private void ExecuteSingleInstantKillHit(SingleHitContext hitContext)
    {
        ShowSkillAttackMessage(hitContext.Attacker, hitContext.Target, hitContext.Skill);
        
        bool instantKillSucceeded = DetermineInstantKillSuccess(hitContext.Attacker, hitContext.Target, hitContext.Skill, hitContext.Affinity);
        
        if (instantKillSucceeded)
        {
            ApplyInstantKillEffect(hitContext);
        }
        else
        {
            ShowInstantKillFailureMessage(hitContext);
        }
    }

    private bool DetermineInstantKillSuccess(IUnit attacker, IUnit target, SkillData skill, string affinity)
    {
        int attackerLck = attacker.GetLck();
        int targetLck = target.GetLck();
        int skillPower = skill.power;
        
        return affinity switch
        {
            "Wk" => true,
            "Nu" => false,
            "Rs" => attackerLck + skillPower >= 2 * targetLck,
            _ => attackerLck + skillPower >= targetLck
        };
    }

    private void ApplyInstantKillEffect(SingleHitContext hitContext)
    {
        ShowInstantKillAffinityMessage(hitContext);
        hitContext.Target.TakeDamage(hitContext.Target.GetCurrentHp());
        _gameUi.ShowInstantKillSuccess(hitContext.Target.GetName());
    }

    private void ShowInstantKillAffinityMessage(SingleHitContext hitContext)
    {
        string attackerName = hitContext.Attacker.GetName();
        string targetName = hitContext.Target.GetName();
        
        switch (hitContext.Affinity)
        {
            case "Wk":
                _gameUi.ShowWeakInstantKillResponse(attackerName, targetName);
                break;
            case "Rs":
                _gameUi.ShowResistInstantKillResponse(attackerName, targetName);
                break;
        }
    }

    private void ShowInstantKillFailureMessage(SingleHitContext hitContext)
    {
        string attackerName = hitContext.Attacker.GetName();
        string targetName = hitContext.Target.GetName();
        
        if (hitContext.Affinity == "Nu")
        {
            _gameUi.ShowNullInstantKillResponse(attackerName, targetName);
        }
        else
        {
            _gameUi.ShowInstantKillFailure(attackerName);
        }
    }

    private bool IsInstantKillSkill(string skillType)
    {
        return skillType == "Light" || skillType == "Dark";
    }

    private void ShowSkillAttackMessage(IUnit attacker, IUnit target, SkillData skill)
    {
        string attackerName = attacker.GetName();
        string targetName = target.GetName();
        string actionVerb = skill.GetSkillActionVerb(skill.type);
        _gameUi.ShowSkillAttack(attackerName, actionVerb, targetName);
    }

    private void ShowFinalSkillResult(IUnit attacker, IUnit target, string affinity)
    {
        string attackerName = attacker.GetName();
        string targetName = target.GetName();

        if (_affinityProcessorController.ShouldShowAttackerResult(affinity))
        {
            _damageApplicator.ShowUnitHealthStatus(attacker, attackerName);
        }
        else
        {
            _damageApplicator.ShowUnitHealthStatus(target, targetName);
        }
    }

    private void ShowDamageResultForAttack(AttackResultContext context)
    {
        if (_affinityProcessorController.ShouldShowAttackerResult(context.Affinity))
        {
            _damageApplicator.ShowUnitHealthStatus(context.Attacker, context.AttackerName);
        }
        else
        {
            _damageApplicator.ShowUnitHealthStatus(context.Target, context.TargetName);
        }
    }
}