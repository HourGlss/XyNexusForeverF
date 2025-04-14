using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Movement;
using NexusForever.Game.Abstract.Entity.Movement.Command;
using NexusForever.Game.Abstract.Entity.Stat;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Entity.Movement.Command.Position;
using NexusForever.Game.Static.Combat.CrowdControl;
using NexusForever.Game.Static.Entity;
using NexusForever.Network.World.Entity;
using NexusForever.Network.World.Entity.Model;
using NexusForever.Shared;

namespace NexusForever.Game.Entity
{
    internal class PickupEntity : UnitEntity, IPickupEntity
    {
        private enum Spell
        {
            DefaultDisarmKnockback = 35921,
            SubdueBeam             = 78470
        }

        public override EntityType Type => EntityType.Pickup;

        private uint itemDisplayId;

        #region Dependency Injection

        private readonly IFactory<ISpellParameters> spellParameterFactory;

        public PickupEntity(IMovementManager movementManager,
            IEntitySummonFactory entitySummonFactory,
            IStatUpdateManager<IUnitEntity> statUpdateManager,
            ISpellFactory spellFactory,
            IFactory<ISpellParameters> spellParameterFactory)
            : base(movementManager, entitySummonFactory, statUpdateManager, spellFactory)
        {
            this.spellParameterFactory = spellParameterFactory;

            statUpdateManager.Initialise(this);
        }

        #endregion

        protected override IEntityModel BuildEntityModel()
        {
            return new PickupEntityModel
            {
                CreatureId    = CreatureId,
                OwnerId       = SummonerGuid ?? 0,
                ItemDisplayId = itemDisplayId
            };
        }

        public void Initialise(IItem item)
        {
            itemDisplayId = item.Info.GetDisplayId();
        }

        /// <summary>
        /// Invoked when <see cref="IPickupEntity"/> is added to <see cref="IBaseMap"/>.
        /// </summary>
        public override void OnAddToMap(IBaseMap map, uint guid, Vector3 vector)
        {
            base.OnAddToMap(map, guid, vector);

            if (SummonerGuid == null)
                return;

            //CastSpell(Spell.DefaultDisarmKnockback, spellParameterFactory.Resolve());

            // TODO: remove once forced movement is implemented, should use spell instead, parameters stolen out of spell effect
            Vector3 position = vector.GetPoint2D((float)Random.Shared.NextDouble() * (MathF.PI * 2f), 15f);
            MovementManager.SetPositionProjectile(400, 100f, position);

            ISpellParameters parameters = spellParameterFactory.Resolve();
            parameters.PrimaryTargetId = SummonerGuid.Value;
            CastSpell(Spell.SubdueBeam, parameters);
        }

        /// <summary>
        /// Invoked when an <see cref="IEntityCommand"/> has finialised for <see cref="IPickupEntity"/>.
        /// </summary>
        public override void OnEntityCommandFinalise(IEntityCommand command)
        {
            base.OnEntityCommandFinalise(command);

            if (command is not PositionProjectileCommand)
                return;

            SetInRangeCheck(3f);
        }

        protected override void AddToRange(IGridEntity entity)
        {
            base.AddToRange(entity);

            if (entity.Guid != SummonerGuid)
                return;

            if (entity is not IPlayer player)
                return;

            ISpellTargetEffectInfo spell = player.CrowdControlManager.GetCCEffect(CCState.Subdue);
            if (spell == null)
                return;

            spell.Finish();
        }
    }
}
