using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Spell.Event;
using NexusForever.Game.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model;
using NexusForever.Shared;
using NexusForever.Shared.Game;

namespace NexusForever.Game.Spell.Type
{
    public class SpellAura : Spell
    {
        public override CastMethod CastMethod => CastMethod.Aura;

        private readonly UpdateTimer auraExecute = new(TimeSpan.FromMilliseconds(100));
        private readonly Dictionary<Spell4EffectsEntry, UpdateTimer> effectRetriggerTimers = [];

        #region Dependency Injection

        private readonly ILogger<SpellAura> log;
        private readonly ISpellTargetInfoCollection spellTargetInfoCollection;

        public SpellAura(
            ILogger<SpellAura> log,
            ISpellTargetInfoCollection spellTargetInfoCollection,
            IGlobalSpellManager globalSpellManager)
            : base(log, spellTargetInfoCollection, globalSpellManager)
        {
            this.log                       = log;
            this.spellTargetInfoCollection = spellTargetInfoCollection;
        }

        #endregion

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public override void Update(double lastTick)
        {
            base.Update(lastTick);

            if (status != SpellStatus.Executing)
                return;

            HandleOutOfRangeTargets(lastTick);
            HandleEffectTicks(lastTick);
        }

        private void HandleOutOfRangeTargets(double lastTick)
        {
            auraExecute.Update(lastTick);
            if (!auraExecute.HasElapsed)
                return;

            var targetCollection = new SpellTargetCollection();
            SelectTargets(targetCollection);

            foreach (ISpellTargetInfo targetInfo in spellTargetInfoCollection)
            {
                ISpellTarget target = targetCollection.GetTarget(targetInfo.Guid, SpellEffectTargetFlags.ImplicitTarget);
                if (target == null)
                {
                    targetInfo.Finish();

                    // this will remove the SpellTargetInfo at the client allowing for it to be created again through a future SpellGo
                    SendBuffsRemoved([targetInfo.Guid]);
                }
            }

            auraExecute.Reset();
        }

        private void HandleEffectTicks(double lastTick)
        {
            var effects = new List<Spell4EffectsEntry>();
            foreach ((Spell4EffectsEntry effect, UpdateTimer updateTimer) in effectRetriggerTimers)
            {
                updateTimer.Update(lastTick);
                if (updateTimer.HasElapsed)
                    effects.Add(effect);
            }

            Execute(effects);
        }

        public override bool Cast()
        {
            if (!base.Cast())
                return false;

            uint castTime = Parameters.CastTimeOverride > -1 ? (uint)Parameters.CastTimeOverride : Parameters.SpellInfo.Entry.CastTime;
            events.EnqueueEvent(new SpellEvent(castTime / 1000d, () =>
            {
                SpellStatus previousStatus = status;
                if ((currentPhase == 0 || currentPhase == 255) && previousStatus != SpellStatus.Executing)
                {
                    CostSpell();
                    SetCooldown();
                }
                Execute(false);
            })); // enqueue spell to be executed after cast time

            foreach (Spell4EffectsEntry effect in Parameters.SpellInfo.Effects.Where(i => i.TickTime > 0))
                effectRetriggerTimers.Add(effect, new UpdateTimer(TimeSpan.FromMilliseconds(effect.TickTime)));

            if (Parameters.SpellInfo.Entry.SpellDuration > 0 && Parameters.SpellInfo.Entry.SpellDuration < uint.MaxValue)
                events.EnqueueEvent(new SpellEvent(Parameters.SpellInfo.Entry.SpellDuration / 1000d, Finish));

            if (Parameters.SpellInfo.BaseInfo.Entry.Creature2IdPositionalAoe > 0)
            {
                var factory = LegacyServiceProvider.Provider.GetService<IEntityFactory>();
                var positionalEntity = factory.CreateEntity<ISimpleEntity>();
                positionalEntity.Initialise(Parameters.SpellInfo.BaseInfo.Entry.Creature2IdPositionalAoe, (entity) =>
                {
                    entity.Rotation = Caster.Rotation;
                    Parameters.PositionalUnitId = entity.Guid;

                    status = SpellStatus.Casting;
                    log.LogTrace($"Spell {Parameters.SpellInfo.Entry.Id} has started casting.");
                });

                Caster.Map.EnqueueAdd(positionalEntity, new Map.MapPosition
                {
                    Info = new Map.MapInfo
                    {
                        Entry = Caster.Map.Entry
                    },
                    Position = Caster.Position
                });
            }
            else
            {
                status = SpellStatus.Casting;
                log.LogTrace($"Spell {Parameters.SpellInfo.Entry.Id} has started casting.");
            }
            return true;
        }

        protected override bool _IsCasting()
        {
            return base._IsCasting() && status == SpellStatus.Casting;
        }

        protected override bool CanExecuteEffect(Spell4EffectsEntry spell4EffectsEntry)
        {
            if (effectRetriggerTimers.TryGetValue(spell4EffectsEntry, out UpdateTimer updateTimer)
                && !updateTimer.HasElapsed)
                return false;

            return base.CanExecuteEffect(spell4EffectsEntry);
        }

        protected override void ExecuteEffect(Spell4EffectsEntry spell4EffectsEntry, ISpellTargetCollection targetCollection)
        {
            base.ExecuteEffect(spell4EffectsEntry, targetCollection);

            if (effectRetriggerTimers.TryGetValue(spell4EffectsEntry, out UpdateTimer updateTimer))
                updateTimer.Reset();
        }

        // TODO: research more, when should this be sent?
        private void SendBuffsApplied(List<uint> unitIds)
        {
            if (unitIds.Count == 0)
                return;

            var serverSpellBuffsApply = new ServerSpellBuffsApply();
            foreach (uint unitId in unitIds)
            {
                serverSpellBuffsApply.spellTargets.Add(new ServerSpellBuffsApply.SpellTarget
                {
                    ServerUniqueId = CastingId,
                    TargetId       = unitId,
                    InstanceCount  = 1 // TODO: If something stacks, we may need to grab this from the target unit
                });
            }

            Caster.EnqueueToVisible(serverSpellBuffsApply, true);
        }

        private void SendBuffsRemoved(List<uint> unitIds)
        {
            if (unitIds.Count == 0)
                return;

            Caster.EnqueueToVisible( new ServerSpellBuffsRemoved
            {
                CastingId    = CastingId,
                SpellTargets = unitIds
            }, true);
        }

        // TODO: research more, when should this be sent?
        private void SendRemoveBuff(uint unitId)
        {
            if (!Parameters.SpellInfo.BaseInfo.HasIcon)
                throw new InvalidOperationException();

            Caster.EnqueueToVisible(new ServerSpellBuffRemove
            {
                CastingId = CastingId,
                CasterId  = unitId
            }, true);
        }
    }
}
