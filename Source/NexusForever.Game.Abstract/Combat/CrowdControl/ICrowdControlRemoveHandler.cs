using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;

namespace NexusForever.Game.Abstract.Combat.CrowdControl
{
    public interface ICrowdControlRemoveHandler
    {
        /// <summary>
        /// Remove the crowd control effect from the <see cref="IUnitEntity"/> target.
        /// </summary>
        void Remove(IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectCCStateSetData data);
    }
}
