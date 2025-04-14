using NexusForever.Game.Static.Spell.Effect;
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

        /// <summary>
        /// Return whether the <see cref="ISpellTargetEffectInfo"/> is pending execution.
        /// </summary>
        /// <remarks>
        /// A pending effect has been sent to the client but has not yet been executed.
        /// </remarks>
        bool IsPending { get; }

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
        SpellEffectExecutionResult Execute(ISpellExecutionContext executionContext);

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
        void AddDamage(IDamageDescription damage);

        /// <summary>
        /// Return the effect duration.
        /// </summary>
        TimeSpan? GetDuration();

        /// <summary>
        /// Set the effect duration.
        /// </summary>
        /// <remarks>
        /// This will overwrite the default duration.
        /// </remarks>
        void SetDuration(TimeSpan? timeSpan);
    }
}
