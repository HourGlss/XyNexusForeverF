namespace NexusForever.Game.Abstract.Spell.Info
{
    public interface ISpellInfoManager
    {
        void Initialise();

        /// <summary>
        /// Return <see cref="ISpellBaseInfo"/> with the supplied spell4BaseId.
        /// </summary>
        ISpellInfo GetSpellInfo(uint spell4Id);

        /// <summary>
        /// Return <see cref="ISpellInfo"/> with the supplied spell4Id.
        /// </summary>
        ISpellBaseInfo GetSpellBaseInfo(uint spell4BaseId);
    }
}
