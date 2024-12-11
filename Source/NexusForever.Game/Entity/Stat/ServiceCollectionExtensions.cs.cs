using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Stat;
using NexusForever.Game.Entity.Stat.Entity;
using NexusForever.Game.Entity.Stat.Player;
using NexusForever.Game.Static.Entity;

namespace NexusForever.Game.Entity.Stat
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGameEntityStat(this IServiceCollection sc)
        {
            sc.AddTransient<IStatUpdateManager<IUnitEntity>, EntityStatUpdateManager>();
            sc.AddTransient<IStatUpdateManager<IPlayer>, PlayerStatUpdateManager>();

            sc.AddTransient<IStatUpdater<IUnitEntity>, HealthStatUpdater>();
            sc.AddTransient<IStatUpdater<IUnitEntity>, SheildStatUpdater>();

            sc.AddTransient<IStatUpdater<IPlayer>, DashStatUpdater>();
            sc.AddTransient<IStatUpdater<IPlayer>, FocusStatUpdater>();
            sc.AddKeyedTransient<IStatUpdater<IPlayer>, WarriorStatUpdater>(Class.Warrior);
            sc.AddKeyedTransient<IStatUpdater<IPlayer>, EngineerStatUpdater>(Class.Engineer);
            sc.AddKeyedTransient<IStatUpdater<IPlayer>, EsperStatUpdater>(Class.Esper);
            sc.AddKeyedTransient<IStatUpdater<IPlayer>, MedicStatUpdater>(Class.Medic);
            sc.AddKeyedTransient<IStatUpdater<IPlayer>, StalkerStatUpdater>(Class.Stalker);
            sc.AddKeyedTransient<IStatUpdater<IPlayer>, SpellSlingerStatUpdater>(Class.Spellslinger);
        }
    }
}
