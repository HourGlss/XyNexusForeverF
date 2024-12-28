using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Stat;

namespace NexusForever.Game.Entity.Stat
{
    public class PlayerStatUpdateManager : EntityStatUpdateManager, IStatUpdateManager<IPlayer>
    {
        #region Dependency Injection

        private readonly IServiceProvider serviceProvider;

        public PlayerStatUpdateManager(
            IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        #endregion

        public void Initialise(IPlayer player)
        {
            Initialise((IUnitEntity)player);

            foreach (var statUpdater in serviceProvider.GetServices<IStatUpdater<IPlayer>>())
                Add(player, statUpdater);

            foreach (var statUpdater in serviceProvider.GetKeyedServices<IStatUpdater<IPlayer>>(player.Class))
                Add(player, statUpdater);
        }
    }
}
