using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Abstract.Spell.Target
{
    public interface ISpellTargetCollection
    {
        /// <summary>
        /// Add <see cref="IUnitEntity"/> target to the collection with the specified <see cref="SpellEffectTargetFlags"/>.
        /// </summary>
        void AddTarget(SpellEffectTargetFlags flags, IUnitEntity entity);

        /// <summary>
        /// Return <see cref="IUnitEntity"/> target with the specified guid and <see cref="SpellEffectTargetFlags"/>.
        /// </summary>
        ISpellTarget GetTarget(uint guid, SpellEffectTargetFlags flags);

        /// <summary>
        /// Return all <see cref="IUnitEntity"/> targets with the specified <see cref="SpellEffectTargetFlags"/>.
        /// </summary>
        IEnumerable<ISpellTarget> GetTargets(SpellEffectTargetFlags flags);
    }
}
