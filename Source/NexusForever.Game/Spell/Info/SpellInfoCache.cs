using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Info
{
    public class SpellInfoCache : ISpellInfoCache
    {
        private Dictionary<uint, List<Spell4Entry>> spellEntries;
        private Dictionary<uint, List<Spell4EffectsEntry>> spellEffectEntries;
        private Dictionary<uint, List<TelegraphDamageEntry>> spellTelegraphEntries;
        private Dictionary<uint, List<Spell4ThresholdsEntry>> spellThresholdEntries;
        private Dictionary<uint, List<SpellPhaseEntry>> spellPhaseEntries;

        #region Dependency Injection

        private readonly IGameTableManager gameTableManager;

        public SpellInfoCache(
            IGameTableManager gameTableManager)
        {
            this.gameTableManager = gameTableManager;
        }

        #endregion

        public void Initialise()
        {
            // caching is required as most of the spell tables have 50k+ entries, calculating for each spell is SLOW
            spellEntries = gameTableManager.Spell4.Entries
                .GroupBy(e => e.Spell4BaseIdBaseSpell)
                .ToDictionary(g => g.Key, g => g
                    .OrderByDescending(e => e.TierIndex)
                    .ToList());

            spellEffectEntries = gameTableManager.Spell4Effects.Entries
                .GroupBy(e => e.SpellId)
                .ToDictionary(g => g.Key, g => g
                    .OrderBy(e => e.OrderIndex)
                    .ToList());

            spellTelegraphEntries = gameTableManager.Spell4Telegraph.Entries
                .GroupBy(e => e.Spell4Id)
                .ToDictionary(g => g.Key, g => g
                    .Select(e => gameTableManager.TelegraphDamage.GetEntry(e.TelegraphDamageId))
                    .Where(e => e != null)
                    .ToList());

            spellThresholdEntries = gameTableManager.Spell4Thresholds.Entries
                .GroupBy(e => e.Spell4IdParent)
                .ToDictionary(g => g.Key, g => g
                    .OrderBy(e => e.OrderIndex)
                    .ToList());

            spellPhaseEntries = gameTableManager.SpellPhase.Entries
                .GroupBy(e => e.Spell4IdOwner)
                .ToDictionary(g => g.Key, g => g
                    .OrderBy(e => e.OrderIndex)
                    .ToList());
        }

        /// <summary>
        /// Return all <see cref="Spell4Entry"/>'s for the supplied spell base id.
        /// </summary>
        public IEnumerable<Spell4Entry> GetSpell4Entries(uint spell4BaseId)
        {
            return spellEntries.TryGetValue(spell4BaseId, out List<Spell4Entry> entries) ? entries : [];
        }

        /// <summary>
        /// Return all <see cref="Spell4EffectsEntry"/>'s for the supplied spell id.
        /// </summary>
        public IEnumerable<Spell4EffectsEntry> GetSpell4EffectEntries(uint spell4Id)
        {
            return spellEffectEntries.TryGetValue(spell4Id, out List<Spell4EffectsEntry> entries) ? entries : [];
        }

        /// <summary>
        /// Return all <see cref="TelegraphDamageEntry"/>'s for the supplied spell id.
        /// </summary>
        public IEnumerable<TelegraphDamageEntry> GetTelegraphDamageEntries(uint spell4Id)
        {
            return spellTelegraphEntries.TryGetValue(spell4Id, out List<TelegraphDamageEntry> entries) ? entries : [];
        }

        /// <summary>
        /// Return all <see cref="Spell4ThresholdsEntry"/>'s for the supplied spell id.
        /// </summary>
        public IEnumerable<Spell4ThresholdsEntry> GetSpell4ThresholdEntries(uint spell4Id)
        {
            return spellThresholdEntries.TryGetValue(spell4Id, out List<Spell4ThresholdsEntry> entries) ? entries : [];
        }

        /// <summary>
        /// Return all <see cref="SpellPhaseEntry"/>'s for the supplied spell id.
        /// </summary>
        public IEnumerable<SpellPhaseEntry> GetSpellPhaseEntries(uint spell4Id)
        {
            return spellPhaseEntries.TryGetValue(spell4Id, out List<SpellPhaseEntry> entries) ? entries : [];
        }
    }
}
