using NexusForever.Game.Static.Reputation;

namespace NexusForever.Game.Abstract.Map.Lock
{
    public interface ITutorialMapLock : IMapLock
    {
        /// <summary>
        /// <see cref="Faction"/> of the <see cref="ITutorialMapLock"/>.
        /// </summary>
        public Faction? Faction { get; }

        /// <summary>
        /// Initialise <see cref="Faction"/> information for <see cref="ITutorialMapLock"/>.
        /// </summary>
        void Initialise(Faction faction);
    }
}
