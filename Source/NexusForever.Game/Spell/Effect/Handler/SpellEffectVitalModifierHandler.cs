using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.Network.World.Combat;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.VitalModifier)]
    public class SpellEffectVitalModifierHandler : ISpellEffectApplyHandler<ISpellEffectVitalModifierData>
    {
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectVitalModifierData data)
        {
            if (data.Vital == Static.Entity.Vital.Invalid)
                return SpellEffectExecutionResult.PreventEffect;

            float previousValue = target.GetVitalValue(data.Vital);
            target.ModifyVital(data.Vital, data.Value);
            float appliedValue = target.GetVitalValue(data.Vital) - previousValue;

            executionContext.AddCombatLog(new CombatLogVitalModifier
            {
                Amount         = appliedValue,
                VitalModified  = (uint)data.Vital,
                BShowCombatLog = appliedValue != 0f,
                CastData       = new CombatLogCastData
                {
                    CasterId     = executionContext.Spell.Caster.Guid,
                    TargetId     = target.Guid,
                    SpellId      = executionContext.Spell.Parameters.SpellInfo.Entry.Id,
                    CombatResult = CombatResult.Hit
                }
            });

            return SpellEffectExecutionResult.Ok;
        }
    }
}
