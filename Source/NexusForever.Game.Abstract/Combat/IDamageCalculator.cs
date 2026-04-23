using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Target;

namespace NexusForever.Game.Abstract.Combat
{
    public interface IDamageCalculator
    {
        /// <summary>
        /// Returns the calculated damage and updates the referenced <see cref="ISpellTargetEffectInfo"/> appropriately.
        /// </summary>
        /// <remarks>
        /// TODO: This should probably return an instance of a Class which describes all the damage done to both entities. Attackers can have reflected damage from this, etc.
        /// </remarks>
        void CalculateDamage(ISpellExecutionContext executionContext, IUnitEntity victim, ISpellTargetEffectInfo info);

        /// <summary>
        /// Calculate the base amount for a spell effect that uses damage-style effect formula fields.
        /// </summary>
        uint CalculateBaseEffectAmount(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info);

        /// <summary>
        /// Try to calculate a secondary Multi-Hit from a successful direct damage effect.
        /// </summary>
        bool TryCalculateMultiHitDamage(ISpellExecutionContext executionContext, IUnitEntity victim, ISpellTargetEffectInfo info, IDamageDescription sourceDamage, out IDamageDescription multiHitDamage);

        /// <summary>
        /// Try to calculate a secondary Multi-Hit from a successful direct heal effect.
        /// </summary>
        bool TryCalculateMultiHitHeal(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, IDamageDescription sourceHeal, out IDamageDescription multiHitHeal);
    }
}
