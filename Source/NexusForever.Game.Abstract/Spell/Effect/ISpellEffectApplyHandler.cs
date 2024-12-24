using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;

namespace NexusForever.Game.Abstract.Spell.Effect
{
    public interface ISpellEffectApplyHandler<T> where T : ISpellEffectData
    {
        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        void Apply(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info, T data);
    }
}
