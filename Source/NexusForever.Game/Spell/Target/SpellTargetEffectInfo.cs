using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model.Shared;
using NexusForever.Shared.Game;

namespace NexusForever.Game.Spell.Target
{
    public class SpellTargetEffectInfo : ISpellTargetEffectInfo
    {
        /// <summary>
        /// Return whether the <see cref="ISpellTargetEffectInfo"/> has been finalised.
        /// </summary>
        /// <remarks>
        /// The effect will be finalised when the duration expires or the effect is prematurely finished.
        /// </remarks>
        public bool IsFinalised { get; private set; }

        public ISpellTargetInfo Target { get; private set; }
        public uint EffectId { get; private set; }
        public Spell4EffectsEntry Entry { get; private set; }
        public IDamageDescription Damage { get; private set; }

        private UpdateTimer duration;

        #region Dependency Injection

        private ILogger<SpellTargetEffectInfo> log;
        private ISpellEffectHandlerInvoker spellEffectHandlerInvoker;
        private IGlobalSpellEffectManager globalSpellEffectManager;

        public SpellTargetEffectInfo(
            ILogger<SpellTargetEffectInfo> log,
            ISpellEffectHandlerInvoker spellEffectHandlerInvoker,
            IGlobalSpellEffectManager globalSpellEffectManager)
        {
            this.log                       = log;
            this.spellEffectHandlerInvoker = spellEffectHandlerInvoker;
            this.globalSpellEffectManager  = globalSpellEffectManager;
        }

        #endregion

        /// <summary>
        /// Initialise <see cref="ISpellTargetEffectInfo"/> with supplied <see cref="ISpellTargetInfo"/> and <see cref="Spell4EffectsEntry"/>.
        /// </summary>
        public void Initialise(ISpellTargetInfo target, Spell4EffectsEntry entry)
        {
            if (EffectId != 0)
                throw new InvalidOperationException("SpellTargetEffectInfo has already been initialised!");

            EffectId = globalSpellEffectManager.NextEffectId;
            Target   = target;
            Entry    = entry;

            if (!entry.Flags.HasFlag(SpellEffectFlags.CancelOnly))
                duration = new UpdateTimer(TimeSpan.FromMilliseconds(entry.DurationTime));

            log.LogTrace($"Initalised effect {Entry.EffectType} for target {Target.Guid} for spell {Target.Collection.Spell.Spell4Id}");
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            if (IsFinalised)
                return;

            duration?.Update(lastTick);
            if (duration != null && duration.HasElapsed)
                Finish();
        }

        public EffectInfo Build()
        {
            var effectInfo = new EffectInfo
            {
                Spell4EffectId = Entry.Id,
                EffectUniqueId = EffectId,
                DelayTime      = Entry.DelayTime,
                TimeRemaining  = duration != null ? (int)TimeSpan.FromSeconds(duration.Duration).TotalMilliseconds : -1
            };

            if (Damage != null)
            {
                effectInfo.InfoType              = 1;
                effectInfo.DamageDescriptionData = Damage.Build();
            }

            return effectInfo;
        }

        /// <summary>
        /// Execute the effect.
        /// </summary>
        /// <remarks>
        /// This will also invoke the <see cref="ISpellEffectApplyHandler"/> for the effect type if it exists.
        /// </remarks>
        public void Execute()
        {
            try
            {
                spellEffectHandlerInvoker.InvokeApplyHandler(Target.Collection.Spell, Target.GetTarget(), this);
            }
            catch (Exception e)
            {
                log.LogError(e, $"An exception occurred applying effect {Entry.EffectType} for target {Target.Guid} for spell {Target.Collection.Spell.Spell4Id}!");
            }

            log.LogTrace($"Applied effect {Entry.EffectType} for target {Target.Guid} for spell {Target.Collection.Spell.Spell4Id}");
        }

        /// <summary>
        /// Finish the effect.
        /// </summary>
        /// <remarks>
        /// This will also invoke the <see cref="ISpellEffectRemoveHandler"/> for the effect type if it exists.
        /// </remarks>
        public void Finish()
        {
            if (IsFinalised)
                return;

            HandleSpellEffectRemove();
            IsFinalised = true;

            log.LogTrace($"Finished effect {Entry.EffectType} for target {Target.Guid} for spell {Target.Collection.Spell.Spell4Id}");
        }

        private void HandleSpellEffectRemove()
        {
            try
            {
                spellEffectHandlerInvoker.InvokeRemoveHandler(Target.Collection.Spell, Target.GetTarget(), this);
            }
            catch (Exception e)
            {
                log.LogError(e, $"An exception occurred removing effect {Entry.EffectType} for target {Target.Guid} for spell {Target.Collection.Spell.Spell4Id}!");
            }

            log.LogTrace($"Removed effect {Entry.EffectType} for target {Target.Guid} for spell {Target.Collection.Spell.Spell4Id}");
        }

        /// <summary>
        /// Add damage to the effect.
        /// </summary>
        public void AddDamage(DamageType damageType, uint damage)
        {
            // TODO: handle this correctly
            Damage = new DamageDescription
            {
                DamageType      = damageType,
                RawDamage       = damage,
                RawScaledDamage = damage,
                AdjustedDamage  = damage,
                CombatResult    = CombatResult.Hit
            };
        }

        /// <summary>
        /// Set the effect duration.
        /// </summary>
        /// <remarks>
        /// This will overwrite the default duration.
        /// </remarks>
        public void SetDuration(TimeSpan timeSpan)
        {
            duration = new UpdateTimer(timeSpan);
        }
    }
}
