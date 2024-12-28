using NexusForever.Game.Abstract;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Movement;
using NexusForever.Game.Abstract.Entity.Stat;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Entity;
using NexusForever.Network.World.Entity;
using NexusForever.Network.World.Entity.Model;

namespace NexusForever.Game.Entity
{
    public class CollectableUnitEntity : UnitEntity, ICollectableUnitEntity
    {
        public override EntityType Type => EntityType.CollectableUnit;

        #region Dependency Injection

        private readonly IAssetManager assetManager;

        public CollectableUnitEntity(
            IMovementManager movementManager,
            IEntitySummonFactory entitySummonFactory,
            IStatUpdateManager<IUnitEntity> statUpdateManager,
            ISpellFactory spellFactory,
            IAssetManager assetManager)
            : base(movementManager, entitySummonFactory, statUpdateManager, spellFactory)
        {
            this.assetManager = assetManager;

            statUpdateManager.Initialise(this);
        }

        #endregion

        protected override IEntityModel BuildEntityModel()
        {
            return new CollectableUnitEntityModel
            {
                CreatureId        = CreatureId,
                QuestChecklistIdx = QuestChecklistIdx
            };
        }
    }
}
