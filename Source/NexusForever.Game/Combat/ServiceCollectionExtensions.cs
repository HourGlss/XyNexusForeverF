using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Spell;
using NexusForever.Shared;

namespace NexusForever.Game.Combat
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGameCombat(this IServiceCollection sc)
        {
            sc.AddTransientFactory<IDamageCalculator, DamageCalculator>();
            sc.AddTransient<IDamageDescription, DamageDescription>();
        }
    }
}
