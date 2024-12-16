using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Spell.Proc;
using NexusForever.GameTable.Model;
using NexusForever.Shared;

namespace NexusForever.Game.Abstract.Spell.Proc
{
    public interface IProcManager : IUpdate
    {
        /// <summary>
        /// Initialise <see cref="IProcManager"/> with supplied <see cref="IUnitEntity"/> owner.
        /// </summary>
        void Initialise(IUnitEntity owner);

        /// <summary>
        /// Apply <see cref="IProcInfo"/> for supplied <see cref="Spell4EffectsEntry"/>.
        /// </summary>
        void ApplyProc(Spell4EffectsEntry entry);

        /// <summary>
        /// Remove <see cref="IProcInfo"/> for supplied <see cref="Spell4EffectsEntry"/>.
        /// </summary>
        void RemoveProc(Spell4EffectsEntry entry);

        /// <summary>
        /// Trigger all <see cref="IProcInfo"/> of the specified <see cref="ProcType"/>.
        /// </summary>
        void TriggerProc(ProcType type);

        /// <summary>
        /// Trigger all <see cref="IProcInfo"/> of the specified <see cref="ProcType"/> with the supplied <see cref="IProcParameters"/>.
        /// </summary>
        void TriggerProc(ProcType type, IProcParameters parameters);
    }
}
