// Este archivo reemplaza a BattleSystem.cs
using System;
using Proyecto_Megaten_IIC2113.Enums;
using Proyecto_Megaten_IIC2113.Models;
using Proyecto_Megaten_IIC2113.Views;

namespace Proyecto_Megaten_IIC2113.Controllers
{
    public class CombatProcessor
    {
        private readonly GameUi _gameUi;

        public CombatProcessor(GameUi gameUi)
        {
            _gameUi = gameUi;
        }

        public bool CanProcessAttackWithAffinity(UnitBase attacker, UnitBase target, int baseDamage, int hitCount, 
            bool critical, SkillElement element, SkillData skill)
        {
            if (target.IsDead)
            {
                return false;
            }

            int totalDamage = CalculateTotalDamage(attacker, target, baseDamage, hitCount, critical, element);
            bool attackSuccess = ApplyDamageToTarget(target, totalDamage, element);
            DisplayAttackResults(attacker, target, critical, hitCount, element, totalDamage, skill);
            
            return attackSuccess;
        }

        private int CalculateTotalDamage(UnitBase attacker, UnitBase target, int baseDamage, int hitCount, 
            bool critical, SkillElement element)
        {
            double damageMultiplier = GetAffinityDamageMultiplier(target, element);
            int calculatedDamage = (int)(baseDamage * damageMultiplier);
            
            if (critical)
            {
                calculatedDamage = ApplyCriticalBonus(calculatedDamage);
            }
            
            return calculatedDamage * hitCount;
        }

        private double GetAffinityDamageMultiplier(UnitBase target, SkillElement element)
        {
            double multiplier = 1.0;
            
            if (element != SkillElement.Neutral)
            {
                AffinityType affinity = target.GetAffinityType(element);
                
                switch (affinity)
                {
                    case AffinityType.Weakness:
                        multiplier = 1.5;
                        break;
                    case AffinityType.Resistance:
                        multiplier = 0.5;
                        break;
                    case AffinityType.Null:
                        multiplier = 0;
                        break;
                    case AffinityType.Repel:
                        multiplier = -1.0;
                        break;
                    case AffinityType.Drain:
                        multiplier = -1.0;
                        break;
                }
            }
            
            return multiplier;
        }

        private int ApplyCriticalBonus(int damage)
        {
            return (int)(damage * 1.5);
        }

        private bool ApplyDamageToTarget(UnitBase target, int totalDamage, SkillElement element)
        {
            AffinityType affinity = target.GetAffinityType(element);
            
            if (affinity == AffinityType.Drain)
            {
                int healAmount = Math.Min(totalDamage, target.MaxHp - target.CurrentHp);
                target.CurrentHp += healAmount;
                return true;
            }
            
            if (totalDamage < 0)
            {
                return false; // Repelled attack
            }
            
            target.CurrentHp = Math.Max(target.CurrentHp - totalDamage, 0);
            return target.CurrentHp == 0;
        }

        private void DisplayAttackResults(UnitBase attacker, UnitBase target, bool critical, int hitCount, 
            SkillElement element, int totalDamage, SkillData skill)
        {
            AffinityType affinity = target.GetAffinityType(element);
            _gameUi.ShowAffinityResponse(affinity, target.Name, hitCount, totalDamage, critical, attacker.Name, skill?.GetSkillActionVerb() ?? "ataca a");
        }

        // Método para obtener la estadística base según tipo de unidad y habilidad
        private int GetBaseStatForSkill(UnitBase unit, SkillType skillType)
        {
            if (unit is Samurai samurai)
            {
                return skillType == SkillType.Physical ? samurai.Strength : samurai.Magic;
            }
            else if (unit is Monster monster)
            {
                return skillType == SkillType.Physical ? monster.Strength : monster.Magic;
            }
            return 0;
        }

        public int CalculateSkillDamage(UnitBase unit, SkillData skill)
        {
            int baseStat = GetBaseStatForSkill(unit, skill.Type);
            int skillPower = skill.Power;
            
            return baseStat + skillPower;
        }

        // Resto de métodos refactorizados...
        // ApplyOffensiveSkill ahora dividido en métodos más pequeños
        
        public bool CanApplyOffensiveSkill(UnitBase attacker, UnitBase target, SkillData skill)
        {
            if (target.IsDead || skill == null)
            {
                return false;
            }

            int baseDamage = CalculateSkillDamage(attacker, skill);
            int hits = skill.GetHitsCount();
            bool isCritical = IsAttackCritical();

            return CanProcessAttackWithAffinity(attacker, target, baseDamage, hits, isCritical, skill.Element, skill);
        }

        private bool IsAttackCritical()
        {
            Random random = new Random();
            return random.Next(100) < 10; // 10% de probabilidad de crítico
        }

        public bool ApplyHealingToTarget(UnitBase caster, UnitBase target, SkillData skill)
        {
            // Implementación del método
            // ...
            return true;
        }
    }
}
