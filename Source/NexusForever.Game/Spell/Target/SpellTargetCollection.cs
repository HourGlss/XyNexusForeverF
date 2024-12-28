using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell.Target
{
    public class SpellTargetCollection : ISpellTargetCollection
    {
        private readonly Dictionary<uint, ISpellTarget> targets = [];

        /// <summary>
        /// Add <see cref="IUnitEntity"/> target to the collection with the specified <see cref="SpellEffectTargetFlags"/>.
        /// </summary>
        public void AddTarget(SpellEffectTargetFlags flags, IUnitEntity entity)
        {
            if (targets.TryGetValue(entity.Guid, out ISpellTarget target))
                target.Flags |= flags;
            else
                targets.Add(entity.Guid, new SpellTarget(entity, flags));
        }

        /// <summary>
        /// Return <see cref="IUnitEntity"/> target with the specified guid and <see cref="SpellEffectTargetFlags"/>.
        /// </summary>
        public ISpellTarget GetTarget(uint guid, SpellEffectTargetFlags flags)
        {
            if (targets.TryGetValue(guid, out ISpellTarget target) && target.Flags.HasFlag(flags))
                return target;

            return null;
        }

        /// <summary>
        /// Return all <see cref="IUnitEntity"/> targets with the specified <see cref="SpellEffectTargetFlags"/>.
        /// </summary>
        public IEnumerable<ISpellTarget> GetTargets(SpellEffectTargetFlags flags)
        {
            return targets.Values.Where(t => (t.Flags & flags) != 0);
        }
    }
}
