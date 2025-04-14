using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Combat.CrowdControl;
using NexusForever.Game.Static.Combat.CrowdControl;

namespace NexusForever.Game.Combat.CrowdControl
{
    public class CrowdControlFactory : ICrowdControlFactory
    {
        #region Dependency Injection

        private readonly IServiceProvider serviceProvider;

        public CrowdControlFactory(
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        #endregion

        /// <summary>
        /// Return the <see cref="ICrowdControlApplyHandler"/> for the given <see cref="CCState"/>.
        /// </summary>
        public ICrowdControlApplyHandler GetApplyHandler(CCState state)
        {
            return serviceProvider.GetKeyedService<ICrowdControlApplyHandler>(state);
        }

        /// <summary>
        /// Return the <see cref="ICrowdControlRemoveHandler"/> for the given <see cref="CCState"/>.
        /// </summary>
        public ICrowdControlRemoveHandler GetRemoveHandler(CCState state)
        {
            return serviceProvider.GetKeyedService<ICrowdControlRemoveHandler>(state);
        }
    }
}
