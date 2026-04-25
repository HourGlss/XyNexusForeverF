using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Abstract.Spell.Validator;
using NexusForever.Game.Spell.Event;
using NexusForever.Game.Spell.Telemetry;
using NexusForever.Game.Static.Spell;
using NexusForever.Network.World.Message.Static;

namespace NexusForever.Game.Spell.Type
{
    public class SpellChanneledField : Spell
    {
        public override CastMethod CastMethod => CastMethod.ChanneledField;

        #region Dependency Injection

        private readonly ILogger<SpellChanneledField> log;

        public SpellChanneledField(
            ILogger<SpellChanneledField> log,
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

            events.EnqueueEvent(new SpellEvent(Parameters.SpellInfo.Entry.ChannelInitialDelay / 1000d, () =>
            {
                CastResult checkResources = CheckResourceConditions();
                if (checkResources != CastResult.Ok)
                {
                    CancelCast(checkResources);
                    return;
                }

                Execute();
            }));

            if (Parameters.SpellInfo.Entry.ChannelPulseTime > 0)
            {
                uint numberOfPulses = (uint)MathF.Floor(Parameters.SpellInfo.Entry.ChannelMaxTime / Parameters.SpellInfo.Entry.ChannelPulseTime);
                for (int i = 1; i <= numberOfPulses; i++)
                {
                    uint pulse = (uint)i;
                    events.EnqueueEvent(new SpellEvent((Parameters.SpellInfo.Entry.ChannelInitialDelay + Parameters.SpellInfo.Entry.ChannelPulseTime * pulse) / 1000d, () =>
                    {
                        CastResult checkResources = CheckResourceConditions();
                        if (checkResources != CastResult.Ok)
                        {
                            CancelCast(checkResources);
                            return;
                        }

                        Execute();
                    }));
                }
            }

            events.EnqueueEvent(new SpellEvent(Parameters.SpellInfo.Entry.ChannelMaxTime / 1000d, Finish));

            status = SpellStatus.Casting;
            log.LogTrace($"SpellChanneledField {Parameters.SpellInfo.Entry.Id} has started casting.");
            return true;
        }

        protected override bool _IsCasting()
        {
            return base._IsCasting() && (status == SpellStatus.Casting || status == SpellStatus.Executing);
        }
    }
}
