using NexusForever.Game.Static.Reputation;

namespace NexusForever.Game.Abstract.Map.Instance
{
    public interface ITutorialMapInstance : IMapInstance
    {
        /// <summary>
        /// <see cref="Faction"/> of the <see cref="ITutorialMapInstance"/>.
        /// </summary>
        Faction Faction { get; }

        /// <summary>
        /// Initialise <see cref="ITutorialMapInstance"/> with <see cref="Faction"/>.
        /// </summary>
        void Initialise(Faction faction);
    }
}
