using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.Proc)]
    public class SpellEffectProcHandler : ISpellEffectApplyHandler, ISpellEffectRemoveHandler
    {
        public void Apply(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info)
        {
            target.ProcManager.ApplyProc(info.Entry);
        }

        public void Remove(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info)
        {
            target.ProcManager.RemoveProc(info.Entry);
        }
    }
}
