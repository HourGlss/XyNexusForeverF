using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.Disguise)]
    public class SpellEffectDisguiseHandler : ISpellEffectApplyHandler<ISpellEffectDisguiseData>, ISpellEffectRemoveHandler<ISpellEffectDisguiseData>
    {
        #region Dependency Injection

        private readonly ICreatureInfoManager creatureInfoManager;

        public SpellEffectDisguiseHandler(
            ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDisguiseData data)
        {
            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(data.CreatureId);
            if (creatureInfo == null)
                return SpellEffectExecutionResult.PreventEffect;

            target.CreatureDisplayEntry = creatureInfo.GetDisplayInfoEntry();
            return SpellEffectExecutionResult.Ok;
        }

        /// <summary>
        /// Handle <see cref="ISpell"/> effect remove on <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Remove(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDisguiseData data)
        {
            target.CreatureDisplayEntry = target.CreatureInfo?.GetDisplayInfoEntry() ?? null;
        }
    }
}
