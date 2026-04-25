using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Abstract.Spell.Validator;
using NexusForever.Game.Spell.Event;
using NexusForever.Game.Spell.Telemetry;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Type
{
    public class SpellMultiphase : Spell
    {
        public override CastMethod CastMethod => CastMethod.Multiphase;

        #region Dependency Injection

        private readonly ILogger<SpellMultiphase> log;

        public SpellMultiphase(
            ILogger<SpellMultiphase> log,
            ISpellTargetInfoCollection spellTargetInfoCollection,
            IGlobalSpellManager globalSpellManager,
            ICastResultValidatorManager castResultValidatorManager,
            IDisableManager disableManager,
            ISpellDiagnostics spellDiagnostics)
            : base(log, spellTargetInfoCollection, globalSpellManager, castResultValidatorManager, disableManager, spellDiagnostics)
        {
            this.log = log;
        }

        #endregion

        public override bool Cast()
        {
            if (!base.Cast())
                return false;

            uint spellDelay = 0;
            for (int i = 0; i < Parameters.SpellInfo.Phases.Count; i++)
            {
                int index = i;
                SpellPhaseEntry spellPhase = Parameters.SpellInfo.Phases[i];
                spellDelay += spellPhase.PhaseDelay;
                events.EnqueueEvent(new SpellEvent(spellDelay / 1000d, () =>
                {
                    currentPhase = (byte)spellPhase.OrderIndex;
                    Execute();

                    if (index == Parameters.SpellInfo.Phases.Count - 1)
                    {
                        status = SpellStatus.Finishing;
                        log.LogTrace($"SpellMultiphase {Parameters.SpellInfo.Entry.Id} has finished executing.");
                    }
                }));
            }

            status = SpellStatus.Casting;
            log.LogTrace($"SpellMultiphase {Parameters.SpellInfo.Entry.Id} has started casting.");
            return true;
        }

        protected override bool _IsCasting()
        {
            return base._IsCasting() && (status == SpellStatus.Casting || status == SpellStatus.Executing);
        }

        protected override bool IsTelegraphValid(TelegraphDamageEntry telegraph)
        {
            // Ensure only telegraphs that apply to this Execute phase are evaluated.
            return IsPhaseFlagMatch(telegraph.PhaseFlags);
        }

        protected override bool CanExecuteEffect(Spell4EffectsEntry spell4EffectsEntry)
        {
            if (!IsPhaseFlagMatch(spell4EffectsEntry.PhaseFlags))
                return false;

            return base.CanExecuteEffect(spell4EffectsEntry);
        }

        private bool IsPhaseFlagMatch(uint phaseFlags)
        {
            return IsPhaseFlagMatch(currentPhase, phaseFlags);
        }

        private static bool IsPhaseFlagMatch(byte phase, uint phaseFlags)
        {
            if (phase >= 255 || phaseFlags == uint.MaxValue)
                return true;

            if (phaseFlags == 0u)
                return false;

            if (phase >= 32)
                return false;

            uint phaseMask = 1u << phase;
            return (phaseFlags & phaseMask) != 0u;
        }
    }
}
