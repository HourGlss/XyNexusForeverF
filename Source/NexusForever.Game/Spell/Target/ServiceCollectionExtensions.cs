using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Abstract.Spell.Target.Implicit;
using NexusForever.Game.Abstract.Spell.Target.Implicit.Filter;
using NexusForever.Game.Spell.Effect;
using NexusForever.Game.Spell.Target.Implicit;
using NexusForever.Game.Spell.Target.Implicit.Filter;
using NexusForever.Shared;

namespace NexusForever.Game.Spell.Target
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGameSpellTarget(this IServiceCollection sc)
        {
            sc.AddTransient<ISpellTargetInfoCollection, SpellTargetInfoCollection>();
            sc.AddTransientFactory<ISpellTargetInfo, SpellTargetInfo>();
            sc.AddTransientFactory<ISpellTargetEffectInfo, SpellTargetEffectInfo>();
            sc.AddTransient<ISpellEffectHandlerFactory, SpellEffectHandlerFactory>();

            sc.AddTransientFactory<ISpellTargetImplicitSelector, SpellTargetImplicitSelector>();
            sc.AddTransientFactory<ISpellTargetImplicitTelegraphFilter, SpellTargetImplicitTelegraphFilter>();
            sc.AddTransientFactory<ISpellTargetImplicitConstraintFilter, SpellTargetImplicitConstraintFilter>();
        }
    }
}
