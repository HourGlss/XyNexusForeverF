using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Spell.Proc;
using NexusForever.GameTable.Model;
using NexusForever.Shared;

namespace NexusForever.Game.Abstract.Spell.Proc
{
    public interface IProcInfo : IUpdate
    {
        IUnitEntity Owner { get; }
        Spell4EffectsEntry Effect { get; }
        ProcType Type { get; }

        /// <summary>
        /// Initialise <see cref="IProcInfo"/> with supplied <see cref="IUnitEntity"/> owner and <see cref="Spell4EffectsEntry"/>.
        /// </summary>
        void Initialise(IUnitEntity owner, Spell4EffectsEntry entry);

        /// <summary>
        /// Trigger the <see cref="IProcInfo"/> with supplied <see cref="IProcParameters"/>.
        /// </summary>
        void Trigger(IProcParameters parameters);
    }
}
