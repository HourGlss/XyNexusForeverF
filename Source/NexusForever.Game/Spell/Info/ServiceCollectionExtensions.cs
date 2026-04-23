using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.Game.Spell.Info.Patch;
using NexusForever.Shared;

namespace NexusForever.Game.Spell.Info
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGameSpellInfo(this IServiceCollection sc)
        {
            sc.AddTransientFactory<ISpellInfo, SpellInfo>();
            sc.AddTransientFactory<ISpellBaseInfo, SpellBaseInfo>();
            sc.AddTransientFactory<ISpellInfoCache, SpellInfoCache>();

            sc.AddSingleton<ISpellInfoManager, SpellInfoManager>();
            sc.AddSingleton<ISpellInfoPatchManager, SpellInfoPatchManager>();

            sc.AddTransient<BioShellVolatilitySpellInfoPatch>();
            sc.AddTransient<PulseBlastSpellInfoPatch>();
            sc.AddTransient<RicochetVolatilitySpellInfoPatch>();
            sc.AddTransient<VolatileInjectionVolatilitySpellInfoPatch>();
        }
    }
}
