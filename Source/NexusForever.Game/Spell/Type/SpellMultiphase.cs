using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Abstract.Spell.Validator;
using NexusForever.Game.Spell.Event;
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
            IDisableManager disableManager)
            : base(log, spellTargetInfoCollection, globalSpellManager, castResultValidatorManager, disableManager)
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

                    if (i == Parameters.SpellInfo.Phases.Count - 1)
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
            if (currentPhase >= 255)
                return true;

            int phaseMask = 1 << currentPhase;
            return telegraph.PhaseFlags != 1 && (phaseMask & telegraph.PhaseFlags) != 0;
        }

        protected override bool CanExecuteEffect(Spell4EffectsEntry spell4EffectsEntry)
        {
            if (currentPhase < 255)
            {
                int phaseMask = 1 << currentPhase;
                if (spell4EffectsEntry.PhaseFlags != 1 && spell4EffectsEntry.PhaseFlags != uint.MaxValue && (phaseMask & spell4EffectsEntry.PhaseFlags) == 0)
                    return false;
            }

            return base.CanExecuteEffect(spell4EffectsEntry);
        }
    }
}
