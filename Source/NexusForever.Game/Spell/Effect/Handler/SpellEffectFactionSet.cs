using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.FactionSet)]
    public class SpellEffectFactionSet : ISpellEffectApplyHandler<ISpellEffectFactionSetData>, ISpellEffectRemoveHandler<ISpellEffectFactionSetData>
    {
        public void Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectFactionSetData data)
        {
            target.SetTemporaryFaction(data.FactionId);
        }

        public void Remove(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectFactionSetData data)
        {
            target.RemoveTemporaryFaction();
        }
    }
}
