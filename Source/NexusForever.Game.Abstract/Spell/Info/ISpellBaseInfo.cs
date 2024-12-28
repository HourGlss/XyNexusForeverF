using NexusForever.Game.Spell.Info;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Abstract.Spell.Info
{
    public interface ISpellBaseInfo : IEnumerable<ISpellInfo>
    {
        Spell4BaseEntry Entry { get; }
        Spell4HitResultsEntry HitResult { get; }
        Spell4TargetMechanicsEntry TargetMechanics { get; }
        Spell4TargetAngleEntry TargetAngle { get; }
        Spell4PrerequisitesEntry Prerequisites { get; }
        Spell4ValidTargetsEntry ValidTargets { get; }
        TargetGroupEntry CastGroup { get; }
        Creature2Entry PositionalAoe { get; }
        TargetGroupEntry AoeGroup { get; }
        Spell4BaseEntry PrerequisiteSpell { get; }
        Spell4SpellTypesEntry SpellType { get; }
        bool HasIcon { get; }
        bool IsDebuff { get; }
        bool IsBuff { get; }
        bool IsDispellable { get; }

        /// <summary>
        /// Initialise the <see cref="ISpellBaseInfo"/> with the supplied <see cref="Spell4BaseEntry"/> and <see cref="ISpellInfoCache"/>.
        /// </summary>
        void Initialise(Spell4BaseEntry spell4BaseEntry, ISpellInfoCache spellInfoCache);

        /// <summary>
        /// Return <see cref="ISpellInfo"/> for the supplied spell tier.
        /// </summary>
        ISpellInfo GetSpellInfo(byte tier);
    }
}