using NexusForever.Game.Abstract.Combat;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.Network.World.Combat;
using NexusForever.Shared;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.Heal)]
    public class SpellEffectHealHandler : ISpellEffectApplyHandler<ISpellEffectDamageData>
    {
        #region Dependency Injection

        private readonly IFactory<IDamageCalculator> damageCalculatorFactory;
        private readonly IFactory<IDamageDescription> damageDescriptionFactory;

        public SpellEffectHealHandler(
            IFactory<IDamageCalculator> damageCalculatorFactory,
            IFactory<IDamageDescription> damageDescriptionFactory)
        {
            this.damageCalculatorFactory  = damageCalculatorFactory;
            this.damageDescriptionFactory = damageDescriptionFactory;
        }

        #endregion

        /// <summary>
        /// Handle <see cref="ISpell"/> heal effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDamageData data)
        {
            if (!executionContext.Spell.Caster.IsAlive || !target.IsAlive)
                return SpellEffectExecutionResult.PreventEffect;

            uint amount     = damageCalculatorFactory.Resolve().CalculateBaseEffectAmount(executionContext, target, info);
            uint missing    = target.MaxHealth - target.Health;
            uint healAmount = Math.Min(amount, missing);
            uint overheal   = amount - healAmount;

            if (healAmount > 0u)
                target.ModifyHealth(healAmount, DamageType.Heal, executionContext.Spell.Caster);

            info.AddDamage(BuildDamageDescription(amount, healAmount));
            executionContext.AddCombatLog(new CombatLogHeal
            {
                HealAmount = healAmount,
                Overheal   = overheal,
                EffectType = SpellEffectType.Heal,
                CastData   = BuildCastData(executionContext.Spell, target)
            });

            if (executionContext.Spell.Caster is IPlayer player)
            {
                player.Map.PublicEventManager.UpdateStat(player, PublicEventStat.Healed, healAmount);
                player.Map.PublicEventManager.UpdateStat(player, PublicEventStat.Overhealed, overheal);
            }

            if (target is IPlayer targetPlayer && overheal > 0u)
                targetPlayer.Map.PublicEventManager.UpdateStat(targetPlayer, PublicEventStat.OverhealingReceived, overheal);

            return SpellEffectExecutionResult.Ok;
        }

        private IDamageDescription BuildDamageDescription(uint rawAmount, uint healAmount)
        {
            IDamageDescription damageDescription = damageDescriptionFactory.Resolve();
            damageDescription.RawDamage       = rawAmount;
            damageDescription.RawScaledDamage = rawAmount;
            damageDescription.AdjustedDamage  = healAmount;
            damageDescription.CombatResult    = CombatResult.Hit;
            damageDescription.DamageType      = DamageType.Heal;
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
