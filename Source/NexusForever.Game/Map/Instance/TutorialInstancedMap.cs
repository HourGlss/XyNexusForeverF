using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.Map.Lock;
using NexusForever.Shared;

namespace NexusForever.Game.Map.Instance
{
    public class TutorialInstancedMap : InstancedMap<ITutorialMapInstance>
    {
        #region Dependency Injection

        private readonly IMapLockManager mapLockManager;
        private readonly IFactory<ITutorialMapInstance> instanceFactory;

        public TutorialInstancedMap(
            ILogger<TutorialInstancedMap> log,
            IMapLockManager mapLockManager,
            IFactory<ITutorialMapInstance> instanceFactory)
            : base(log)
        {
            this.mapLockManager  = mapLockManager;
            this.instanceFactory = instanceFactory;
        }

        #endregion

        protected override ITutorialMapInstance CreateInstance(IPlayer player, IMapLock mapLock)
        {
            ITutorialMapInstance tutorialInstance = instanceFactory.Resolve();
            tutorialInstance.Initialise(Entry, mapLock);
            tutorialInstance.Initialise(player.Faction2);
            return tutorialInstance;
        }

        protected override IMapLock GetMapLock(IPlayer player)
        {
            return mapLockManager.GetTutorialLock(player.Faction2);
        }
    }
}
