using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.VitalModifier)]
    public class SpellEffectVitalModifierHandler : ISpellEffectApplyHandler<ISpellEffectVitalModifierData>
    {
        public void Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectVitalModifierData data)
        {
            target.ModifyVital(data.Vital, data.Value);
        }
    }
}
