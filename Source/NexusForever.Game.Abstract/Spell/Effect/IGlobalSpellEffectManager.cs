using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Abstract.Spell.Effect
{
    public delegate SpellEffectExecutionResult SpellEffectHandlerApplyDelegate(object handler, ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectData data);
    public delegate void SpellEffectHandlerRemoveDelegate(object handler, ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectData data);

    public interface IGlobalSpellEffectManager
    {
        /// <summary>
        /// Id to be assigned to the next spell effect.
        /// </summary>
        uint NextEffectId { get; }

        void Initialise();

        /// <summary>
        /// Get spell effect data <see cref="Type"/> for supplied <see cref="SpellEffectType"/>.
        /// </summary>
        Type GetSpellEffectDataType(SpellEffectType type);

        /// <summary>
        /// Get spell effect apply handler <see cref="Type"/> for supplied <see cref="SpellEffectType"/>.
        /// </summary>
        Type GetSpellEffectApplyHandlerType(SpellEffectType type);

        /// <summary>
        /// Get spell effect remove handler <see cref="Type"/> for supplied <see cref="SpellEffectType"/>.
        /// </summary>
        Type GetSpellEffectRemoveHandlerType(SpellEffectType type);

        /// <summary>
        /// Get spell effect apply handler <see cref="SpellEffectHandlerApplyDelegate"/> for supplied <see cref="SpellEffectType"/>.
        /// </summary>
        SpellEffectHandlerApplyDelegate GetSpellEffectApplyDelegate(SpellEffectType type);

        /// <summary>
        /// Get spell effect remove handler <see cref="SpellEffectHandlerRemoveDelegate"/> for supplied <see cref="SpellEffectType"/>.
        /// </summary>
        SpellEffectHandlerRemoveDelegate GetSpellEffectRemoveDelegate(SpellEffectType type);
    }
}
