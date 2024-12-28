using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Static.Spell.Proc;
using NexusForever.Shared;

namespace NexusForever.Game.Abstract.Spell.Proc
{
    public interface IProcInfo : IUpdate
    {
        IUnitEntity Owner { get; }
        ISpellEffectProcData Data { get; }
        ProcType Type { get; }

        /// <summary>
        /// Initialise <see cref="IProcInfo"/> with supplied <see cref="IUnitEntity"/> owner and <see cref="ISpellEffectProcData"/>.
        /// </summary>
        void Initialise(IUnitEntity owner, ISpellEffectProcData data);

        /// <summary>
        /// Trigger the <see cref="IProcInfo"/> with supplied <see cref="IProcParameters"/>.
        /// </summary>
        void Trigger(IProcParameters parameters);
    }
}
