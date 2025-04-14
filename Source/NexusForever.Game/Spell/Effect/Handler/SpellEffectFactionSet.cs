using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.FactionSet)]
    public class SpellEffectFactionSet : ISpellEffectApplyHandler<ISpellEffectFactionSetData>, ISpellEffectRemoveHandler<ISpellEffectFactionSetData>
    {
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectFactionSetData data)
        {
            target.SetTemporaryFaction(data.FactionId);
            return SpellEffectExecutionResult.Ok;
        }

        public void Remove(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectFactionSetData data)
        {
            target.RemoveTemporaryFaction();
        }
    }
}
