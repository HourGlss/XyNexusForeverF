using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model.Shared;
using NexusForever.Shared;

namespace NexusForever.Game.Spell.Target
{
    public class SpellTargetInfo : ISpellTargetInfo
    {
        /// <summary>
        /// Return whether the <see cref="ISpellTargetInfo"/> has been finalised.
        /// </summary>
        /// <remarks>
        /// The target will be finalised when all effects are finialised.
        /// </remarks>
        public bool IsFinalised { get; private set; }

        public ISpellTargetInfoCollection Collection { get; private set; }
        public byte Index { get; private set; }
        public uint Guid { get; private set; }
        public SpellEffectTargetFlags Flags { get; private set; }

        private readonly Dictionary<uint, ISpellTargetEffectInfo> effects = [];

        #region Dependency Injection

        private readonly ILogger<SpellTargetInfo> log;
        private readonly IFactory<ISpellTargetEffectInfo> spellTargetEffectInfoFactory;

        public SpellTargetInfo(
            ILogger<SpellTargetInfo> log,
            IFactory<ISpellTargetEffectInfo> spellTargetEffectInfoFactory)
        {
            this.log                          = log;
            this.spellTargetEffectInfoFactory = spellTargetEffectInfoFactory;
        }

        #endregion

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            if (IsFinalised)
                return;

            var toRemove = new List<ISpellTargetEffectInfo>();
            foreach (ISpellTargetEffectInfo effect in effects.Values)
            {
                effect.Update(lastTick);
                if (effect.IsFinalised)
                    toRemove.Add(effect);
            }

            RemoveFinalised(toRemove);
        }

        private void RemoveFinalised(List<ISpellTargetEffectInfo> toRemove)
        {
            if (toRemove.Count == 0)
                return;

            foreach (ISpellTargetEffectInfo effect in toRemove)
            {
                effects.Remove(effect.Entry.Id);
                log.LogTrace($"Removed SpellTargetEffectInfo for target {Guid} for spell {Collection.Spell.Spell4Id}.");
            }

            if (effects.Count == 0)
            {
                IsFinalised = true;
                log.LogTrace($"Finalised SpellTargetInfo for target {Guid} for spell {Collection.Spell.Spell4Id}.");
            }
        }

        /// <summary>
        /// Initialise <see cref="ISpellTargetInfo"/> with supplied <see cref="ISpellTargetInfoCollection"/>, index and <see cref="ISpellTarget"/>.
        /// </summary>
        public void Initialise(ISpellTargetInfoCollection collection, byte index, ISpellTarget target)
        {
            if (Collection != null)
                throw new InvalidOperationException("SpellTargetInfo has already been initialised!");

            Collection = collection;
            Index      = index;
            Guid       = target.Entity.Guid;
            Flags      = target.Flags;

            log.LogTrace($"Initialised SpellTargetInfo for target {Guid} for spell {Collection.Spell.Spell4Id}.");
        }

        public TargetInfo Build()
        {
            var targetInfo = new TargetInfo
            {
                UnitId        = Guid,
                Ndx           = Index,
                TargetFlags   = Flags,
                InstanceCount = 1,
                CombatResult  = CombatResult.Hit
            };

            foreach (ISpellTargetEffectInfo targetEffectInfo in effects.Values)
                if (targetEffectInfo.Entry.EffectType != SpellEffectType.Proxy)
                    targetInfo.EffectInfoData.Add(targetEffectInfo.Build());

            return targetInfo;
        }

        /// <summary>
        /// Create and execute the supplied <see cref="Spell4EffectsEntry"/>.
        /// </summary>
        public void Execute(Spell4EffectsEntry entry)
        {
            if (!effects.TryGetValue(entry.Id, out ISpellTargetEffectInfo info))
            {
                info = spellTargetEffectInfoFactory.Resolve();
                info.Initialise(this, entry);
                effects.Add(info.Entry.Id, info);

                log.LogTrace($"Added new SpellTargetEffectInfo for target {Guid} for spell {Collection.Spell.Spell4Id}.");
            }

            info.Execute();
        }

        /// <summary>
        /// Finish all effects for the target.
        /// </summary>
        public void Finish()
        {
            if (IsFinalised)
                return;

            foreach (ISpellTargetEffectInfo targetEffectInfo in effects.Values)
                targetEffectInfo.Finish();

            log.LogTrace($"Finished all SpellTargetEffectInfo for target {Guid} spell {Collection.Spell.Spell4Id}.");
        }

        /// <summary>
        /// Return the <see cref="IUnitEntity"/> for the target.
        /// </summary>
        /// <remarks>
        /// This will attempt to find the <see cref="IUnitEntity"/> from visible units of the caster. 
        /// </remarks>
        public IUnitEntity GetTarget()
        {
            return Collection.Spell.Caster.GetVisible<IUnitEntity>(Guid);
        }
    }
}
