using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.GameTable;
using NexusForever.Script.Main.AI;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Event;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Dungeon.ColdbloodCitadel.Script
{
    [ScriptFilterScriptName("HailstoneGatecrasherEntityScript")]
    public class HailstoneGatecrasherEntityScript : CombatAI
    {
        private enum Spell
        {
            FrigidTemperaturesAuraTier1        = 87803,
            IceRageLAETier1                    = 87899,
            FreezePulseProxyTier1              = 87958,
            HowlingWindsVectorSlideAuraTier1   = 87975,
            HowlingWindsBaseTier1              = 88021,
            HowlingWindsShardProxy1Tier1       = 88023,
            HowlingWindsShardProxy2ATier1      = 88030,
            HowlingWindsShardProxy2BTier1      = 88031,
            HowlingWindsShardProxy2CTier1      = 88032,
            HowlingWindsIceSpikesAuraTier1     = 88036,
            MoOTier1                           = 88037,
            HowlingWindsFailDamageTier1        = 88038,
            HowlingWindsMeleeKnockbackTier1    = 88039,
            SlingSnowBouldersProgTier1         = 88042,
            PoundOfIceSRE1Tier1                = 88043,
            EnrageTier1                        = 88070,
            SlingSnowBouldersTargetSelectTier1 = 88190,
            PellKillTier1                      = 88191,
            FreezeBlastEnrageSpellTier1        = 88358
        }

        #region Dependency Injection

        private readonly IScriptEventFactory eventFactory;
        private readonly IScriptEventManager eventManager;

        public HailstoneGatecrasherEntityScript(
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
            autoAttacks = [88044, 88045]; //look at sniffs
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
            entity.Map.PublicEventManager.UpdateObjective(PublicEventObjectiveType.KillEventObjectiveUnit, 14447, 1);

            entity.RemoveFromMap();
        }
    }
}
