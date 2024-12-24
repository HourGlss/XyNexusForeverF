using NexusForever.Game.Static.Spell;
using NexusForever.GameTable.Model;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Shared;
using NexusForever.Shared;

namespace NexusForever.Game.Abstract.Spell.Target
{
    public interface ISpellTargetEffectInfo : IUpdate, INetworkBuildable<EffectInfo>
    {
        /// <summary>
        /// Return whether the <see cref="ISpellTargetEffectInfo"/> has been finalised.
        /// </summary>
        /// <remarks>
        /// The effect will be finalised when the duration expires or the effect is prematurely finished.
        /// </remarks>
        bool IsFinalised { get; }

        ISpellTargetInfo Target { get; }
        uint EffectId { get; }
        Spell4EffectsEntry Entry { get; }
        IDamageDescription Damage { get; }

        /// <summary>
        /// Initialise <see cref="ISpellTargetEffectInfo"/> with supplied <see cref="ISpellTargetInfo"/> and <see cref="Spell4EffectsEntry"/>.
        /// </summary>
        /// <remarks>
        /// This will also invoke the <see cref="ISpellEffectApplyHandler"/> for the effect type if it exists.
        /// </remarks>
        void Initialise(ISpellTargetInfo target, Spell4EffectsEntry entry);

        /// <summary>
        /// Execute the effect.
        /// </summary>
        /// <remarks>
        /// This will also invoke the <see cref="ISpellEffectApplyHandler"/> for the effect type if it exists.
        /// </remarks>
        void Execute();

        /// <summary>
        /// Finish the effect.
        /// </summary>
        /// <remarks>
        /// This will also invoke the <see cref="ISpellEffectRemoveHandler"/> for the effect type if it exists.
        /// </remarks>
        void Finish();

        /// <summary>
        /// Add damage to the effect.
        /// </summary>
        void AddDamage(DamageType damageType, uint damage);

        /// <summary>
        /// Set the effect duration.
        /// </summary>
        /// <remarks>
        /// This will overwrite the default duration.
        /// </remarks>
        void SetDuration(TimeSpan timeSpan);
    }
}
