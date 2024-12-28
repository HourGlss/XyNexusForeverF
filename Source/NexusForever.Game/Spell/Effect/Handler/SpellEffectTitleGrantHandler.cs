using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.TitleGrant)]
    public class SpellEffectTitleGrantHandler : ISpellEffectApplyHandler<ISpellEffectTitleGrantData>
    {
        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectTitleGrantData data)
        {
            if (target is not IPlayer player)
                return;

            player.TitleManager.AddTitle((ushort)data.TitleId);
        }
    }
}
