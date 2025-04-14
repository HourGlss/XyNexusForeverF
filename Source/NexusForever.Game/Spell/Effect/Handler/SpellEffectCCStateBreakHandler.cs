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
    [SpellEffectHandler(SpellEffectType.CCStateBreak)]
    public class SpellEffectCCStateBreakHandler : ISpellEffectApplyHandler<ISpellEffectCCStateBreakData>
    {
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectCCStateBreakData data)
        {
            ISpellTargetEffectInfo effect = target.CrowdControlManager.GetCCEffect(data.CCState);
            if (effect == null)
                return SpellEffectExecutionResult.Ok;

            effect.Finish();

            executionContext.AddCombatLog(new CombatLogCCStateBreak
            {
                CasterId = target.Guid,
                State    = data.CCState
            });

            return SpellEffectExecutionResult.Ok;
        }
    }
}
