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
    [SpellEffectHandler(SpellEffectType.DamageShields)]
    public class SpellEffectDamageShieldsHandler : ISpellEffectApplyHandler<ISpellEffectDamageData>
    {
        #region Dependency Injection

        private readonly IFactory<IDamageCalculator> damageCalculatorFactory;
        private readonly IFactory<IDamageDescription> damageDescriptionFactory;

        public SpellEffectDamageShieldsHandler(
            IFactory<IDamageCalculator> damageCalculatorFactory,
            IFactory<IDamageDescription> damageDescriptionFactory)
        {
            this.damageCalculatorFactory  = damageCalculatorFactory;
            this.damageDescriptionFactory = damageDescriptionFactory;
        }

        #endregion

        /// <summary>
        /// Handle <see cref="ISpell"/> shield damage effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDamageData data)
        {
            if (!executionContext.Spell.Caster.CanAttack(target))
                return SpellEffectExecutionResult.PreventEffect;

            uint amount       = damageCalculatorFactory.Resolve().CalculateBaseEffectAmount(executionContext, target, info);
            uint shieldAmount = Math.Min(amount, target.Shield);

            if (shieldAmount > 0u)
                target.Shield -= shieldAmount;

            info.AddDamage(BuildDamageDescription(info, amount, shieldAmount));
            executionContext.AddCombatLog(new CombatLogDamageShield
            {
                MitigatedDamage = shieldAmount,
                RawDamage       = amount,
                Shield          = shieldAmount,
                DamageType      = info.Entry.DamageType,
                EffectType      = SpellEffectType.DamageShields,
                CastData        = BuildCastData(executionContext.Spell, target)
            });

            return SpellEffectExecutionResult.Ok;
        }

        private IDamageDescription BuildDamageDescription(ISpellTargetEffectInfo info, uint rawAmount, uint shieldAmount)
        {
            IDamageDescription damageDescription = damageDescriptionFactory.Resolve();
            damageDescription.RawDamage          = rawAmount;
            damageDescription.RawScaledDamage    = rawAmount;
            damageDescription.ShieldAbsorbAmount = shieldAmount;
            damageDescription.AdjustedDamage     = shieldAmount;
            damageDescription.CombatResult       = CombatResult.Hit;
            damageDescription.DamageType         = info.Entry.DamageType;
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
