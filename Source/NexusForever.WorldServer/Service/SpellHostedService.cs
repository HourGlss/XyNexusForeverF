using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Info;

namespace NexusForever.WorldServer.Service
{
    public class SpellHostedService : IHostedService
    {
        #region Dependency Injection

        private readonly ISpellInfoPatchManager spellInfoPatchManager;
        private readonly ISpellInfoManager spellInfoManager;
        private readonly IGlobalSpellManager globalSpellManager;
        private readonly IGlobalSpellEffectManager globalSpellEffectManager;

        public SpellHostedService(
            ISpellInfoPatchManager spellInfoPatchManager,
            ISpellInfoManager spellInfoManager,
            IGlobalSpellManager globalSpellManager,
            IGlobalSpellEffectManager globalSpellEffectManager)
        {
            this.spellInfoPatchManager    = spellInfoPatchManager;
            this.spellInfoManager         = spellInfoManager;
            this.globalSpellManager       = globalSpellManager;
            this.globalSpellEffectManager = globalSpellEffectManager;
        }

        #endregion

        public Task StartAsync(CancellationToken cancellationToken)
        {
            spellInfoPatchManager.Initialise();
            spellInfoManager.Initialise();
            globalSpellManager.Initialise();
            globalSpellEffectManager.Initialise();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
