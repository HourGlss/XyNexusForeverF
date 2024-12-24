using System.Collections;
using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Network.World.Message.Model.Shared;
using NexusForever.Shared;

namespace NexusForever.Game.Spell.Target
{
    public class SpellTargetInfoCollection : ISpellTargetInfoCollection
    {
        /// <summary>
        /// Returns whether the <see cref="ISpellTargetInfoCollection"/> has been finalised.
        /// </summary>
        /// <remarks>
        /// The collection will be finalised when no target info remains. 
        /// </remarks>
        public bool IsFinalised => spellTargetInfo.Count == 0;

        public ISpell Spell { get; private set; }

        private readonly Dictionary<uint, ISpellTargetInfo> spellTargetInfo = [];

        #region Dependency Injection

        private readonly ILogger<SpellTargetInfoCollection> log;
        private readonly IFactory<ISpellTargetInfo> spellTargetInfoFactory;

        public SpellTargetInfoCollection(
            ILogger<SpellTargetInfoCollection> log,
            IFactory<ISpellTargetInfo> spellTargetInfoFactory)
        {
            this.log                    = log;
            this.spellTargetInfoFactory = spellTargetInfoFactory;
        }

        #endregion

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            if (IsFinalised)
                return;

            var toRemove = new List<ISpellTargetInfo>();
            foreach (ISpellTargetInfo target in spellTargetInfo.Values)
            {
                target.Update(lastTick);
                if (target.IsFinalised)
                    toRemove.Add(target);
            }

            foreach (ISpellTargetInfo target in toRemove)
            {
                spellTargetInfo.Remove(target.Guid);
                log.LogTrace($"Removed SpellTargetInfo for target {target.Guid} for spell {Spell.Spell4Id}.");
            }
        }

        /// <summary>
        /// Initialises the <see cref="ISpellTargetInfoCollection"/> with the supplied <see cref="ISpell"/>.
        /// </summary>
        public void Initialise(ISpell spell)
        {
            if (Spell != null)
                throw new InvalidOperationException("SpellTargetInfoCollection has already been initialised!");

            Spell = spell;

            log.LogTrace($"Initialised SpellTargetInfoCollection for spell {Spell.Spell4Id}.");
        }

        /// <summary>
        /// Cancel any pending <see cref="ISpellTargetInfo"/>'s in the collection and finalise.
        /// </summary>
        public void Cancel()
        {
            if (IsFinalised)
                return;

            foreach (ISpellTargetInfo item in spellTargetInfo.Values)
                item.Finish();

            log.LogTrace($"Cancelled pending SpellTargetInfo's for spell {Spell.Spell4Id}.");
        }

        /// <summary>
        /// Create a new <see cref="ISpellTargetInfo"/> for the supplied <see cref="ISpellTarget"/>.
        /// </summary>
        public ISpellTargetInfo CreateSpellTargetInfo(ISpellTarget spellTarget)
        {
            ISpellTargetInfo targetInfo = spellTargetInfoFactory.Resolve();
            targetInfo.Initialise(this, (byte)spellTargetInfo.Count, spellTarget);
            spellTargetInfo.Add(spellTarget.Entity.Guid, targetInfo);

            log.LogTrace($"Added new SpellTargetInfo for target {spellTarget.Entity.Guid} for spell {Spell.Spell4Id}.");

            return targetInfo;
        }


        /// <summary>
        /// Return an existing <see cref="ISpellTargetInfo"/> for the supplied <see cref="ISpellTarget"/>.
        /// </summary>
        public ISpellTargetInfo GetSpellTargetInfo(ISpellTarget spellTarget)
        {
            return spellTargetInfo.TryGetValue(spellTarget.Entity.Guid, out ISpellTargetInfo targetInfo) ? targetInfo : null;
        }

        public IEnumerator<ISpellTargetInfo> GetEnumerator()
        {
            return spellTargetInfo.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
