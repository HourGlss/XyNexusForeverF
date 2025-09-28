using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Game.Static.Spell;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Event;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Expedition.EvilFromTheEther.Script
{
    public abstract class EthericPortalEntityScript : INonPlayerScript, IOwnedScript<INonPlayerEntity>
    {
        private enum Creature
        {
            TetheredCreature = 71133,
        }

        private INonPlayerEntity entity;

        private uint portalCount;

        #region Dependency Injection

        private readonly IScriptEventFactory eventFactory;
        private readonly IScriptEventManager eventManager;
        private readonly ICreatureInfoManager creatureInfoManager;
        private readonly IFactory<ISpellParameters> spellParameterFactory;

        public EthericPortalEntityScript(
            IScriptEventFactory eventFactory,
            IScriptEventManager eventManager,
            ICreatureInfoManager creatureInfoManager,
            IFactory<ISpellParameters> spellParameterFactory)
        {
            this.eventFactory          = eventFactory;
            this.eventManager          = eventManager;
            this.creatureInfoManager   = creatureInfoManager;
            this.spellParameterFactory = spellParameterFactory;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(INonPlayerEntity owner)
        {
            entity = owner;
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            eventManager.Update(lastTick);
        }

        protected void CreateTetheredOrganism(TimeSpan time, float angle)
        {
            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(Creature.TetheredCreature);
            if (creatureInfo == null)
                return;

            float entityAngle = -entity.Rotation.X;
            entityAngle -= MathF.PI / 2;
            Vector3 position = entity.Position.GetPoint2D(entityAngle + angle, 5f);

            var @event = eventFactory.CreateEvent<IEntitySummonEvent>();
            @event.Initialise(entity.SummonFactory, creatureInfo, position, entity.Rotation);
            eventManager.EnqueueEvent(time, @event);
        }

        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> summons another <see cref="IWorldEntity"/>.
        /// </summary>
        /// <param name="entity"></param>
        public void OnSummon(IWorldEntity summoned)
        {
            portalCount++;
        }

        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> unsummons another <see cref="IWorldEntity"/>.
        /// </summary>
        public void OnUnsummon(IWorldEntity summoned)
        {
            portalCount--;

            // TODO: implement, this spell should shrink and damage the portal
            //ISpellParameters parameters = spellParameterFactory.Resolve();
            //entity.CastSpell(81642, parameters);

            if (portalCount == 0)
                entity.ModifyHealth(entity.Health, DamageType.Magic, null);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to <see cref="IBaseMap"/>.
        /// </summary>
        public abstract void OnAddToMap(IBaseMap map);

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is remove from <see cref="IBaseMap"/>.
        /// </summary>
        public void OnRemoveFromMap(IBaseMap map)
        {
            eventManager.CancelEvents();
        }

        /// <summary>
        /// Invoked when <see cref="IUnitEntity"/> is killed.
        /// </summary>
        public void OnDeath()
        {
            entity.Map.PublicEventManager.UpdateObjective(PublicEventObjectiveType.Script, 0, 1);
            entity.RemoveFromMap();
        }
    }
}
