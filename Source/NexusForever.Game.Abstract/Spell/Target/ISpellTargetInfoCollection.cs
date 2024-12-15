using NexusForever.Shared;

namespace NexusForever.Game.Abstract.Spell.Target
{
    public interface ISpellTargetInfoCollection : IUpdate, IEnumerable<ISpellTargetInfo>
    {
        /// <summary>
        /// Returns whether the <see cref="ISpellTargetInfoCollection"/> has been finalised.
        /// </summary>
        /// <remarks>
        /// The collection will be finalised when no target info remains. 
        /// </remarks>
        bool IsFinalised { get; }

        ISpell Spell { get; }

        /// <summary>
        /// Initialises the <see cref="ISpellTargetInfoCollection"/> with the supplied <see cref="ISpell"/>.
        /// </summary>
        void Initialise(ISpell spell);

        /// <summary>
        /// Cancel any pending <see cref="ISpellTargetInfo"/>'s in the collection and finalise.
        /// </summary>
        void Cancel();

        /// <summary>
        /// Return or create a <see cref="ISpellTargetInfo"/> for the supplied <see cref="ISpellTarget"/>.
        /// </summary>
        ISpellTargetInfo GetSpellTargetInfo(ISpellTarget spellTarget);
    }
}
