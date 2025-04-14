using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Abstract.Spell.Effect
{
    public interface ISpellEffectHandlerInvoker
    {
        /// <summary>
        /// Invoke the apply handler for the given spell effect.
        /// </summary>
        SpellEffectExecutionResult InvokeApplyHandler(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info);

        /// <summary>
        /// Invoke the remove handler for the given spell effect.
        /// </summary>
        void InvokeRemoveHandler(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info);
    }
}
