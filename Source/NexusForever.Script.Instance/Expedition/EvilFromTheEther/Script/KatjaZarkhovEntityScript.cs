using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Main.AI;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Event;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Expedition.EvilFromTheEther.Script
{
    [ScriptFilterScriptName("KatjaZarkhovEntityScript")]
    public class KatjaZarkhovEntityScript : CombatAI
    {
        private enum Spell
        {
            ClawedFury     = 56037,
            SlicingWind    = 56358,
            PouncingSlice  = 56390,
            TurnedRavenous = 81706,
            RavenousBurst  = 82855
        }

        private enum Creature
        {
            EtherDriveSchematics = 71821,
        }

        private bool hasEnraged;

        #region Dependency Injection

        private readonly IScriptEventFactory eventFactory;
        private readonly IScriptEventManager eventManager;
        private readonly IEntitySummonFactory entitySummonFactory;
        private readonly ICreatureInfoManager creatureInfoManager;

        public KatjaZarkhovEntityScript(
            IFactory<ISpellParameters> spellParametersFactory,
            IGameTableManager gameTableManager,
            IScriptEventFactory eventFactory,
            IScriptEventManager eventManager,
            IEntitySummonFactory entitySummonFactory,
            ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager)
        {
            this.eventFactory        = eventFactory;
            this.entitySummonFactory = entitySummonFactory;
            this.creatureInfoManager = creatureInfoManager;

            this.eventManager        = eventManager;
            this.eventManager.OnScriptEvent += OnScriptEvent;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            autoAttacks = [2605, 2607];

            entitySummonFactory.Initialise(owner);
        }

        protected override void UpdateAI(double lastTick)
        {
            base.UpdateAI(lastTick);
            eventManager.Update(lastTick);
        }

        private void OnScriptEvent(IScriptEvent scriptEvent, uint? _)
        {
            switch (scriptEvent)
            {
                case IEntityCastEvent castEvent:
                    OnEntityCastEvent(castEvent);
                    break;
            }
        }

        private void OnEntityCastEvent(IEntityCastEvent @event)
        {
            switch ((Spell)@event.SpellId)
            {
                case Spell.ClawedFury:
                case Spell.SlicingWind:
                case Spell.PouncingSlice:
                    eventManager.EnqueueEvent(TimeSpan.FromSeconds(20), @event);
                    break;
            }
        }

        /// <summary>
        /// Invoked when <see cref="IUnitEntity"/> enters combat.
        /// </summary>
        public override void OnEnterCombat()
        {
            IEntityCastEvent clawedFuryEvent = eventFactory.CreateEvent<IEntityCastEvent>();
            clawedFuryEvent.Initialise(entity, Spell.ClawedFury, false);
            eventManager.EnqueueEvent(TimeSpan.FromSeconds(8), clawedFuryEvent);
        }

        /// <summary>
        /// Invoked when <see cref="IUnitEntity"/> leaves combat.
        /// </summary>
        public override void OnLeaveCombat()
        {
            eventManager.CancelEvents();
            hasEnraged = false;
        }

        /// <summary>
        /// Invoked when health is changed by source <see cref="IUnitEntity"/>.
        /// </summary>
        public override void OnHealthChange(IUnitEntity source, uint amount, DamageType? type)
        {
            base.OnHealthChange(source, amount, type);

            // enrage at 55% health
            if (((float)entity.Health) / ((float)entity.MaxHealth) < 0.55 && !hasEnraged)
                Enrage();
        }

        private void Enrage()
        {
            eventManager.CancelEvents<IEntityCastEvent>();

            IEntityCastEvent ravenousBurstEvent = eventFactory.CreateEvent<IEntityCastEvent>();
            ravenousBurstEvent.Initialise(entity, 82855, false);
            eventManager.EnqueueEvent(TimeSpan.FromSeconds(0), ravenousBurstEvent);

            IEntityCastEvent turnedRavenousEvent = eventFactory.CreateEvent<IEntityCastEvent>();
            turnedRavenousEvent.Initialise(entity, 81706, false);
            eventManager.EnqueueEvent(TimeSpan.FromSeconds(7), turnedRavenousEvent);

            IEntityCastEvent pounchingSliceEvent = eventFactory.CreateEvent<IEntityCastEvent>();
            pounchingSliceEvent.Initialise(entity, 56390, true);
            eventManager.EnqueueEvent(TimeSpan.FromSeconds(10), pounchingSliceEvent);

            IEntityCastEvent slicingWindEvent = eventFactory.CreateEvent<IEntityCastEvent>();
            slicingWindEvent.Initialise(entity, 56358, false);
            eventManager.EnqueueEvent(TimeSpan.FromSeconds(20), slicingWindEvent);

            hasEnraged = true;
        }

        /// <summary>
        /// Invoked when <see cref="IUnitEntity"/> is killed.
        /// </summary>
        public override void OnDeath()
        {
            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(Creature.EtherDriveSchematics);
            if (creatureInfo == null)
                return;

            entitySummonFactory.Summon(creatureInfo, entity.Position, entity.Rotation);
        }
    }
}
