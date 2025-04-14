using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;

namespace NexusForever.Game.Abstract.Combat.CrowdControl
{
    public interface ICrowdControlApplyHandler
    {
        /// <summary>
        /// Apply crowd control effect to the <see cref="IUnitEntity"/> target.
        /// </summary>
        void Apply(IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectCCStateSetData data);
    }
}
