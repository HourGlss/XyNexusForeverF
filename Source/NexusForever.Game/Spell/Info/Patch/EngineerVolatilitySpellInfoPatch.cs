using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.Game.Abstract.Spell.Info.Patch;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Info.Patch
{
    public abstract class EngineerVolatilitySpellInfoPatch : ISpellInfoPatch
    {
        private readonly ISpellInfoPatchManager spellInfoManager;

        protected EngineerVolatilitySpellInfoPatch(ISpellInfoPatchManager spellInfoManager)
        {
            this.spellInfoManager = spellInfoManager;
        }

        public abstract void Patch(ISpellInfo spellInfo);

        protected void AddSingleTargetVolatilityProxy(ISpellInfo spellInfo, uint spell4Id, uint phaseFlags = uint.MaxValue)
        {
            spellInfo.Effects.Add(new Spell4EffectsEntry
            {
                Id          = spellInfoManager.NextSpellEffectId,
                EffectType  = SpellEffectType.Proxy,
                TargetFlags = SpellEffectTargetFlags.ImplicitTarget,
                PhaseFlags  = phaseFlags,
                DataBits00  = spell4Id,
                DataBits04  = 1
            });
        }

        protected void AddPeriodicCasterVolatilityProxy(ISpellInfo spellInfo, uint spell4Id, uint durationTime, uint tickTime)
        {
            spellInfo.Effects.Add(new Spell4EffectsEntry
            {
                Id           = spellInfoManager.NextSpellEffectId,
                EffectType   = SpellEffectType.Proxy,
                TargetFlags  = SpellEffectTargetFlags.Caster,
                DurationTime = durationTime,
                TickTime     = tickTime,
                PhaseFlags   = uint.MaxValue,
                DataBits01   = spell4Id,
                DataBits03   = BitConverter.SingleToUInt32Bits(1f),
                DataBits04   = uint.MaxValue
            });
        }
    }

    [SpellInfoPatchSpellBaseId(25538)]
    public class BioShellVolatilitySpellInfoPatch : EngineerVolatilitySpellInfoPatch
    {
        public BioShellVolatilitySpellInfoPatch(ISpellInfoPatchManager spellInfoManager)
            : base(spellInfoManager)
        {
        }

        public override void Patch(ISpellInfo spellInfo)
        {
            AddSingleTargetVolatilityProxy(spellInfo, 35967);
        }
    }

    [SpellInfoPatchSpellBaseId(25626)]
    public class RicochetVolatilitySpellInfoPatch : EngineerVolatilitySpellInfoPatch
    {
        public RicochetVolatilitySpellInfoPatch(ISpellInfoPatchManager spellInfoManager)
            : base(spellInfoManager)
        {
        }

        public override void Patch(ISpellInfo spellInfo)
        {
            AddSingleTargetVolatilityProxy(spellInfo, 35741, 2);
        }
    }

    [SpellInfoPatchSpellBaseId(25951)]
    public class VolatileInjectionVolatilitySpellInfoPatch : EngineerVolatilitySpellInfoPatch
    {
        public VolatileInjectionVolatilitySpellInfoPatch(ISpellInfoPatchManager spellInfoManager)
            : base(spellInfoManager)
        {
        }

        public override void Patch(ISpellInfo spellInfo)
        {
            AddPeriodicCasterVolatilityProxy(spellInfo, 42145, 10000, 1000);
        }
    }
}
