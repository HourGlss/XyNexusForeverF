using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Spell.Event;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Type
{
    public class SpellChargeRelease : SpellThreshold
    {
        public override CastMethod CastMethod => CastMethod.ChargeRelease;

        #region Dependency Injection

        private readonly ILogger<SpellChargeRelease> log;

        public SpellChargeRelease(
            ILogger<SpellChargeRelease> log,
            ISpellTargetInfoCollection spellTargetInfoCollection,
            IGlobalSpellManager globalSpellManager,
            ISpellFactory spellFactory)
            : base(log, spellTargetInfoCollection, globalSpellManager, spellFactory)
        {
            this.log = log;
        }

        #endregion

        public override bool Cast()
        {
            if (status == SpellStatus.Waiting)
                return base.Cast();

            if (!base.Cast())
                return false;

            if (Parameters.ParentSpellInfo == null)
            {
                totalThresholdTimer = (uint)(Parameters.SpellInfo.Entry.ThresholdTime / 1000d);

                // Keep track of cast time increments as we create timers to adjust thresholdValue
                uint nextCastTime = 0;

                // Create timers for each thresholdEntry's timer increment
                foreach (Spell4ThresholdsEntry thresholdsEntry in Parameters.SpellInfo.Thresholds)
                {
                    nextCastTime += thresholdsEntry.ThresholdDuration;

                    if (thresholdsEntry.OrderIndex == 0)
                        continue;

                    events.EnqueueEvent(new SpellEvent(Parameters.SpellInfo.Entry.CastTime / 1000d + nextCastTime / 1000d, () =>
                    {
                        thresholdValue = thresholdsEntry.OrderIndex;
                        SendThresholdUpdate();
                    }));
                }
            }

            events.EnqueueEvent(new SpellEvent(Parameters.SpellInfo.Entry.CastTime / 1000d, Execute)); // enqueue spell to be executed after cast time

            status = SpellStatus.Casting;
            log.LogTrace($"Spell {Parameters.SpellInfo.Entry.Id} has started casting.");
            return true;
        }

        protected override bool _IsCasting()
        {
            return base._IsCasting() && (status == SpellStatus.Casting || status == SpellStatus.Executing || status == SpellStatus.Waiting); ;
        }
    }
}
