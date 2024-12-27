using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Info
{
    public interface ISpellInfoCache
    {
        void Initialise();

        /// <summary>
        /// Return all <see cref="Spell4Entry"/>'s for the supplied spell base id.
        /// </summary>
        IEnumerable<Spell4Entry> GetSpell4Entries(uint spell4BaseId);

        /// <summary>
        /// Return all <see cref="Spell4EffectsEntry"/>'s for the supplied spell id.
        /// </summary>
        IEnumerable<Spell4EffectsEntry> GetSpell4EffectEntries(uint spell4Id);

        /// <summary>
        /// Return all <see cref="TelegraphDamageEntry"/>'s for the supplied spell id.
        /// </summary>
        IEnumerable<TelegraphDamageEntry> GetTelegraphDamageEntries(uint spell4Id);

        /// <summary>
        /// Return all <see cref="Spell4ThresholdsEntry"/>'s for the supplied spell id.
        /// </summary>
        IEnumerable<Spell4ThresholdsEntry> GetSpell4ThresholdEntries(uint spell4Id);

        /// <summary>
        /// Return all <see cref="SpellPhaseEntry"/>'s for the supplied spell id.
        /// </summary>
        IEnumerable<SpellPhaseEntry> GetSpellPhaseEntries(uint spell4Id);
    }
}
