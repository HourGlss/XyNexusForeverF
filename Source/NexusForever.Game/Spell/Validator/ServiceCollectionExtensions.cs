using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Spell.Validator;

namespace NexusForever.Game.Spell.Validator
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGameSpellValidator(this IServiceCollection sc)
        {
            sc.AddTransient<ICastResultValidatorManager, CastResultValidatorManager>();

            sc.AddTransient<ICastResultValidator, CCCastResultValidator>();
        }
    }
}
