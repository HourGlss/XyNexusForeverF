using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Entity;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.UnitPropertyModifier)]
    public class SpellEffectUnitPropertyModifierHandler : ISpellEffectApplyHandler<ISpellEffectUnitPropertyModifierData>, ISpellEffectRemoveHandler<ISpellEffectUnitPropertyModifierData>
    {
        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectUnitPropertyModifierData data)
        {
            // TODO: I suppose these could be cached somewhere instead of generating them every single effect?
            var modifier = new SpellPropertyModifier(data.Property, data.Priority, data.PercentageModifier, data.FlatValueModifier, data.LevelScalingModifier);
            target.AddSpellModifierProperty(modifier, executionContext.Spell.Parameters.SpellInfo.Entry.Id);
        }

        /// <summary>
        /// Handle <see cref="ISpell"/> effect remove on <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Remove(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectUnitPropertyModifierData data)
        {
            target.RemoveSpellProperty(data.Property, spell.Parameters.SpellInfo.Entry.Id);
        }
    }
}
