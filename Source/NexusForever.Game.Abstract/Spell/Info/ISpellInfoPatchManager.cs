namespace NexusForever.Game.Abstract.Spell.Info
{
    public interface ISpellInfoPatchManager
    {
        uint NextSpellEffectId { get; }

        void Initialise();

        /// <summary>
        /// Patch the supplied <see cref="ISpellBaseInfo"/> with any defined matching patches.
        /// </summary>
        void Patch(ISpellBaseInfo spellBaseInfo);

        /// <summary>
        /// Patch the supplied <see cref="ISpellInfo"/> with any defined matching patches.
        /// </summary>
        void Patch(ISpellInfo spellInfo);
    }
}
