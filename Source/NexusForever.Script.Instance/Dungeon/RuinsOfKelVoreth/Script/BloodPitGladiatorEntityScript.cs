using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.GameTable;
using NexusForever.Script.Main.AI;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Event;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Dungeon.RuinsOfKelVoreth.Script
{
    [ScriptFilterScriptName("BloodPitGladiatorEntityScript")]
    public class BloodPitGladiatorEntityScript : CombatAI
    {
        private enum Spell
        {
        }

        #region Dependency Injection

        private readonly IScriptEventFactory eventFactory;
        private readonly IScriptEventManager eventManager;

        public BloodPitGladiatorEntityScript(
            IFactory<ISpellParameters> spellParametersFactory,
            IGameTableManager gameTableManager,
            IScriptEventFactory eventFactory,
            IScriptEventManager eventManager)
            : base(spellParametersFactory, gameTableManager)
        {
            this.eventFactory = eventFactory;
            this.eventManager = eventManager;
            this.eventManager.OnScriptEvent += OnScriptEvent;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            autoAttacks = [32920, 32921];//get real ones
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
            }
        }

        /// <summary>
        /// Invoked when <see cref="IUnitEntity"/> enters combat.
        /// </summary>
        public override void OnEnterCombat()
        {
        }

        /// <summary>
        /// Invoked when <see cref="IUnitEntity"/> leaves combat.
        /// </summary>
        public override void OnLeaveCombat()
        {
            eventManager.CancelEvents();
        }

        /// <summary>
        /// Invoked when <see cref="IUnitEntity"/> is killed.
        /// </summary>
        public override void OnDeath()
        {
            entity.Map.PublicEventManager.UpdateObjective(PublicEventObjectiveType.KillTargetGroup, 3972, 1);
        }
    }
}
