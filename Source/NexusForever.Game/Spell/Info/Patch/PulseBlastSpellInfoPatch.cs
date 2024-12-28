using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.Game.Abstract.Spell.Info.Patch;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Info.Patch
{
    [SpellInfoPatchSpellBaseId(26468)]
    public class PulseBlastSpellInfoPatch : ISpellInfoPatch
    {
        #region Dependency Injection

        private readonly ISpellInfoPatchManager spellInfoManager;

        public PulseBlastSpellInfoPatch(
            ISpellInfoPatchManager spellInfoManager)
        {
            this.spellInfoManager = spellInfoManager;
        }

        #endregion

        public void Patch(ISpellInfo spellInfo)
        {
            // this patch corrects the Engineer spell "Pulse Blast" not generating volatility
            // this spell is marked as "Hidden" in the game tables and it seems we need to add the proxy effect manually
            spellInfo.Effects.Add(new Spell4EffectsEntry
            {
                Id          = spellInfoManager.NextSpellEffectId,
                EffectType  = SpellEffectType.Proxy,
                TargetFlags = SpellEffectTargetFlags.ImplicitTarget,
                PhaseFlags  = 2,     // same phase as the damage proxy effect
                DataBits00  = 42148, // [Hidden] Engineer - Add 15 Volatility
                DataBits04  = 1      // limit to only a single execution, prevents multiple volatility gains when hitting multiple targets
            });
        }
    }
}
