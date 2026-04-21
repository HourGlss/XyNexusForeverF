using NexusForever.Game.Abstract.Combat;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.Network.World.Combat;
using NexusForever.Shared;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.HealShields)]
    public class SpellEffectHealShieldsHandler : ISpellEffectApplyHandler<ISpellEffectDamageData>
    {
        #region Dependency Injection

        private readonly IFactory<IDamageCalculator> damageCalculatorFactory;
        private readonly IFactory<IDamageDescription> damageDescriptionFactory;

        public SpellEffectHealShieldsHandler(
            IFactory<IDamageCalculator> damageCalculatorFactory,
            IFactory<IDamageDescription> damageDescriptionFactory)
        {
            this.damageCalculatorFactory  = damageCalculatorFactory;
            this.damageDescriptionFactory = damageDescriptionFactory;
        }

        #endregion

        /// <summary>
        /// Handle <see cref="ISpell"/> shield heal effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDamageData data)
        {
            if (!executionContext.Spell.Caster.IsAlive || !target.IsAlive)
                return SpellEffectExecutionResult.PreventEffect;

            uint amount       = damageCalculatorFactory.Resolve().CalculateBaseEffectAmount(executionContext, target, info);
            uint missing      = target.MaxShieldCapacity - target.Shield;
            uint shieldAmount = Math.Min(amount, missing);
            uint overheal     = amount - shieldAmount;

            if (shieldAmount > 0u)
                target.Shield += shieldAmount;

            info.AddDamage(BuildDamageDescription(amount, shieldAmount));
            executionContext.AddCombatLog(new CombatLogHeal
            {
                HealAmount = shieldAmount,
                Overheal   = overheal,
                EffectType = SpellEffectType.HealShields,
                CastData   = BuildCastData(executionContext.Spell, target)
            });

            return SpellEffectExecutionResult.Ok;
        }

        private IDamageDescription BuildDamageDescription(uint rawAmount, uint shieldAmount)
        {
            IDamageDescription damageDescription = damageDescriptionFactory.Resolve();
            damageDescription.RawDamage       = rawAmount;
            damageDescription.RawScaledDamage = rawAmount;
            damageDescription.AdjustedDamage  = shieldAmount;
            damageDescription.CombatResult    = CombatResult.Hit;
            damageDescription.DamageType      = DamageType.HealShields;
            return damageDescription;
        }

        private static CombatLogCastData BuildCastData(ISpell spell, IUnitEntity target)
        {
            return new CombatLogCastData
            {
                CasterId     = spell.Caster.Guid,
                TargetId     = target.Guid,
                SpellId      = spell.Parameters.SpellInfo.Entry.Id,
                CombatResult = CombatResult.Hit
            };
        }
    }
}
