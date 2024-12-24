using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Abstract.Spell.Effect
{
    public delegate void SpellEffectHandlerDelegate(object handler, ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectData data);

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
        /// Get spell effect apply handler <see cref="SpellEffectHandlerDelegate"/> for supplied <see cref="SpellEffectType"/>.
        /// </summary>
        SpellEffectHandlerDelegate GetSpellEffectApplyDelegate(SpellEffectType type);

        /// <summary>
        /// Get spell effect remove handler <see cref="SpellEffectHandlerDelegate"/> for supplied <see cref="SpellEffectType"/>.
        /// </summary>
        SpellEffectHandlerDelegate GetSpellEffectRemoveDelegate(SpellEffectType type);
    }
}
