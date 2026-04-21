using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.TitleRevoke)]
    public class SpellEffectTitleRevokeHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (target is not IPlayer player)
                return SpellEffectExecutionResult.PreventEffect;

            if (data.DataBits00 == 0u || data.DataBits00 > ushort.MaxValue)
                return SpellEffectExecutionResult.PreventEffect;

            ushort titleId = (ushort)data.DataBits00;
            if (!player.TitleManager.HasTitle(titleId))
                return SpellEffectExecutionResult.Ok;

            player.TitleManager.RevokeTitle(titleId);
            return SpellEffectExecutionResult.Ok;
        }
    }
}
