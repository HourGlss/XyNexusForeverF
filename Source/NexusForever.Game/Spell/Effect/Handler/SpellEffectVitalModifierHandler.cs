using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.VitalModifier)]
    public class SpellEffectVitalModifierHandler : ISpellEffectApplyHandler<ISpellEffectVitalModifierData>
    {
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectVitalModifierData data)
        {
            target.ModifyVital(data.Vital, data.Value);
            return SpellEffectExecutionResult.Ok;
        }
    }
}
