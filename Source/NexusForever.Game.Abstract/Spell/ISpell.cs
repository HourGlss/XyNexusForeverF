using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Spell;
using NexusForever.Network.World.Message.Model.Shared;
using NexusForever.Network.World.Message.Static;
using NexusForever.Shared;

namespace NexusForever.Game.Abstract.Spell
{
    public interface ISpell : IDisposable, IUpdate
    {
        CastMethod CastMethod { get; }

        ISpellParameters Parameters { get; }
        IUnitEntity Caster { get; }
        uint CastingId { get; }
        uint Spell4Id { get; }

        bool IsCasting { get; }
        bool IsFinished { get; }
        bool IsFailed { get; }
        bool IsWaiting { get; }


        /// <summary>
        /// Initialise <see cref="ISpell"/> with the supplied <see cref="IUnitEntity"/> and <see cref="ISpellParameters"/>.
        /// </summary>
        void Initialise(IUnitEntity caster, ISpellParameters parameters);

        /// <summary>
        /// Begin cast, checking prerequisites before initiating.
        /// </summary>
        bool Cast();

        /// <summary>
        /// Invoked each world tick, after Update() for this <see cref="ISpell"/>, with the delta since the previous tick occurred.
        /// </summary>
        void LateUpdate(double lastTick);

        /// <summary>
        /// Cancel cast with supplied <see cref="CastResult"/>.
        /// </summary>
        void CancelCast(CastResult result);

        /// <summary>
        /// Finish this <see cref="ISpell"/> and end all effects associated with it.
        /// </summary>
        void Finish();

        bool IsMovingInterrupted();

        /// <summary>
        /// Add a <see cref="IProxy"/> to this spell's execution queue.
        /// </summary>
        /// <param name="proxy">Proxy instance to add</param>
        void AddProxy(IProxy proxy);

        /// <summary>
        /// Returns number of times a certain effect has been triggered, for this spell cast, with a given ID.
        /// </summary>
        /// <param name="effectId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        bool GetEffectTriggerCount(uint effectId, out uint count);

        SpellInit BuildSpellInit();
    }
}