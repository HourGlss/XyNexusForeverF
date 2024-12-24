using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.Proc)]
    public class SpellEffectProcHandler : ISpellEffectApplyHandler<ISpellEffectProcData>, ISpellEffectRemoveHandler<ISpellEffectProcData>
    {
        public void Apply(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectProcData data)
        {
            target.ProcManager.ApplyProc(data);
        }

        public void Remove(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectProcData data)
        {
            target.ProcManager.RemoveProc(data);
        }
    }
}
