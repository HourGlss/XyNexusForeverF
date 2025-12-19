using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Map.Lock;
using NexusForever.Game.Static.Reputation;

namespace NexusForever.Game.Map.Lock
{
    public class TutorialMapLock : MapLock, ITutorialMapLock
    {
        /// <summary>
        /// <see cref="Faction"/> of the <see cref="ITutorialMapLock"/>.
        /// </summary>
        public Faction? Faction { get; private set; }

        #region Dependency Injection

        public TutorialMapLock(
            ILogger<TutorialMapLock> log)
            : base(log)
        {
        }

        #endregion

        /// <summary>
        /// Initialise <see cref="Faction"/> information for <see cref="ITutorialMapLock"/>.
        /// </summary>
        public void Initialise(Faction faction)
        {
            if (Faction != null)
                throw new InvalidOperationException();

            Faction = faction;
            log.LogTrace($"Set faction {faction} for tutorial instance {InstanceId}");
        }
    }
}
