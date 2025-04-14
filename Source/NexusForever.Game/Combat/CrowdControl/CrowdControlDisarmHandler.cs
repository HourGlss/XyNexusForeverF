using System.Numerics;
using NexusForever.Game.Abstract.Combat.CrowdControl;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Combat.CrowdControl;
using NexusForever.Game.Static.Entity;
using NexusForever.Network.World.Message.Model.Shared;

namespace NexusForever.Game.Combat.CrowdControl
{
    public class CrowdControlDisarmHandler : ICrowdControlApplyHandler, ICrowdControlRemoveHandler
    {
        #region Dependency Injection

        private readonly ICreatureInfoManager creatureInfoManager;

        public CrowdControlDisarmHandler(
            ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        /// <summary>
        /// Apply crowd control effect to the <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Apply(IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectCCStateSetData data)
        {
            if (target is not IPlayer player)
                return;

            IItem primaryWeapon = player.Inventory.GetItem(new ItemLocation
            {
                Location = InventoryLocation.Equipped,
                BagIndex = (uint)EquippedItem.WeaponPrimary
            });
            if (primaryWeapon == null)
                return;

            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(CCEntity.DisarmedWeapon);
            if (creatureInfo == null)
                return;

            IPickupEntity entity = player.SummonFactory.Summon<IPickupEntity>(creatureInfo, target.Position, Vector3.Zero);
            entity.Initialise(primaryWeapon);
        }

        /// <summary>
        /// Remove the crowd control effect from the <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Remove(IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectCCStateSetData data)
        {
            target.SummonFactory.UnsummonCreature(CCEntity.DisarmedWeapon);
        }
    }
}
