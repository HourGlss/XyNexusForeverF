using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Spell.Target;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell
{
    public class SpellExecutionContext : ISpellExecutionContext
    {
        public ISpell Spell { get; private set; }
        public ISpellTargetCollection TargetCollection { get; } = new SpellTargetCollection();

        private readonly List<Spell4EffectsEntry> spellEffects = [];
        private readonly List<IProxy> proxies = [];
        private Dictionary<uint /*effectId*/, uint/*count*/> effectTriggerCount = [];

        /// <summary>
        /// Initialise the spell execution context with the supplied <see cref="ISpell"/>.
        /// </summary>
        public void Initialise(ISpell spell)
        {
            if (Spell != null)
                throw new InvalidOperationException("SpellExecutionContext has already been initialised.");

            Spell = spell;
        }

        /// <summary>
        /// Add <see cref="Spell4EffectsEntry"/> to the spell execution.
        /// </summary>
        public void AddSpellEffect(Spell4EffectsEntry entry)
        {
            spellEffects.Add(entry);
        }

        /// <summary>
        /// Get all <see cref="Spell4EffectsEntry"/> for spell execution.
        /// </summary>
        public IEnumerable<Spell4EffectsEntry> GetSpellEffects()
        {
            return spellEffects;
        }

        /// <summary>
        /// Add <see cref="IProxy"/> to the spell execution.
        /// </summary>
        public void AddProxy(IProxy proxy)
        {
            proxies.Add(proxy);
        }

        /// <summary>
        /// Get all <see cref="IProxy"/> from spell execution.
        /// </summary>
        public IEnumerable<IProxy> GetProxies()
        {
            return proxies;
        }

        /// <summary>
        /// Increment effect trigger count of supplied effect id for spell execution.
        /// </summary>
        public void IncrementEffectTriggerCount(uint effectId)
        {
            if (effectTriggerCount.ContainsKey(effectId))
                effectTriggerCount[effectId]++;
            else
                effectTriggerCount[effectId] = 1;
        }

        /// <summary>
        /// Returns number of times a certain effect has been triggered, for this spell cast, with a given ID.
        /// </summary>
        public bool GetEffectTriggerCount(uint effectId, out uint count)
        {
            return effectTriggerCount.TryGetValue(effectId, out count);
        }
    }
}
