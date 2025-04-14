using System.Numerics;
using NexusForever.Game.Abstract.Combat.CrowdControl;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Entity.Creature;
using NexusForever.Game.Static.Combat.CrowdControl;

namespace NexusForever.Game.Combat.CrowdControl
{
    public class CrowdControlTetherHandler : ICrowdControlApplyHandler, ICrowdControlRemoveHandler
    {
        #region Dependency Injection

        private readonly ICreatureInfoManager creatureInfoManager;

        public CrowdControlTetherHandler(
            ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        public void Apply(IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectCCStateSetData data)
        {
            CCEntity creatureId = CCEntity.TetherAnchor;
            if (data.CCStateAdditionalDataEntry?.DataInt00 != 0)
                creatureId = (CCEntity)data.CCStateAdditionalDataEntry.DataInt00;

            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(creatureId);
            if (creatureInfo == null)
                return;

            ICreatureInfo creatureInfoOverride = new CreatureInfoOverride()
                .SetCreatureInfoOverride(creatureInfo)
                .SetLevelOverride(target.Level);

            target.SummonFactory.Summon<ITetherEntity>(creatureInfoOverride, target.Position, Vector3.Zero);
        }

        public void Remove(IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectCCStateSetData data)
        {
            CCEntity creatureId = CCEntity.TetherAnchor;
            if (data.CCStateAdditionalDataEntry?.DataInt00 != 0)
                creatureId = (CCEntity)data.CCStateAdditionalDataEntry.DataInt00;

            target.SummonFactory.UnsummonCreature(creatureId);
        }
    }
}
