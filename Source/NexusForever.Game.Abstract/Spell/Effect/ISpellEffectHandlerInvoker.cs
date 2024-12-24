using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell.Target;

namespace NexusForever.Game.Abstract.Spell.Effect
{
    public interface ISpellEffectHandlerInvoker
    {
        /// <summary>
        /// Invoke the apply handler for the given spell effect.
        /// </summary>
        void InvokeApplyHandler(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info);

        /// <summary>
        /// Invoke the remove handler for the given spell effect.
        /// </summary>
        void InvokeRemoveHandler(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info);
    }
}
