using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable.Model;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Shared;
using NexusForever.Shared;

namespace NexusForever.Game.Abstract.Spell.Target
{
    public interface ISpellTargetInfo : IUpdate, INetworkBuildable<TargetInfo>
    {
        /// <summary>
        /// Return whether the <see cref="ISpellTargetInfo"/> has been finalised.
        /// </summary>
        /// <remarks>
        /// The target will be finalised when all effects are finialised.
        /// </remarks>
        bool IsFinalised { get; }

        public ISpellTargetInfoCollection Collection { get; }
        uint Guid { get; }
        SpellEffectTargetFlags Flags { get; }

        /// <summary>
        /// Initialise <see cref="ISpellTargetInfo"/> with supplied <see cref="ISpellTargetInfoCollection"/>, index and <see cref="ISpellTarget"/>.
        /// </summary>
        void Initialise(ISpellTargetInfoCollection collection, byte index, ISpellTarget target);

        /// <summary>
        /// Create and execute the supplied <see cref="Spell4EffectsEntry"/>.
        /// </summary>
        void Execute(Spell4EffectsEntry entry);

        /// <summary>
        /// Finish all effects for the target.
        /// </summary>
        void Finish();

        /// <summary>
        /// Return the <see cref="IUnitEntity"/> for the target.
        /// </summary>
        /// <remarks>
        /// This will attempt to find the <see cref="IUnitEntity"/> from visible units of the caster. 
        /// </remarks>
        IUnitEntity GetTarget();
    }
}