using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Combat.CrowdControl;
using NexusForever.Game.Static.Combat.CrowdControl;

namespace NexusForever.Game.Combat.CrowdControl
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGameCombatCrowdControl(this IServiceCollection sc)
        {
            sc.AddTransient<ICrowdControlManager, CrowdControlManager>();

            sc.AddTransient<ICrowdControlFactory, CrowdControlFactory>();
            sc.AddKeyedTransient<ICrowdControlApplyHandler, CrowdControlStunHandler>(CCState.Stun);
            sc.AddKeyedTransient<ICrowdControlApplyHandler, CrowdControlDisarmHandler>(CCState.Subdue);
            sc.AddKeyedTransient<ICrowdControlRemoveHandler, CrowdControlDisarmHandler>(CCState.Subdue);
            sc.AddKeyedTransient<ICrowdControlApplyHandler, CrowdControlTetherHandler>(CCState.Tether);
            sc.AddKeyedTransient<ICrowdControlRemoveHandler, CrowdControlTetherHandler>(CCState.Tether);
        }
    }
}
