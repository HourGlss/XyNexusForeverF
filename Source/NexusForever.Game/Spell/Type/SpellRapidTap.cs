using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Abstract.Spell.Validator;
using NexusForever.Game.Spell.Event;
using NexusForever.Game.Spell.Type;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell
{
    public class SpellRapidTap : SpellThreshold
    {
        public override CastMethod CastMethod => CastMethod.RapidTap;

        #region Dependency Injection

        private readonly ILogger<SpellRapidTap> log;

        public SpellRapidTap(
            ILogger<SpellRapidTap> log,
            ISpellTargetInfoCollection spellTargetInfoCollection,
            IGlobalSpellManager globalSpellManager,
            ICastResultValidatorManager castResultValidatorManager,
            IDisableManager disableManager,
            ISpellFactory spellFactory)
            : base(log, spellTargetInfoCollection, globalSpellManager, castResultValidatorManager, disableManager, spellFactory)
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
                events.EnqueueEvent(new SpellEvent(Parameters.SpellInfo.Entry.CastTime / 1000d + Parameters.SpellInfo.Entry.ThresholdTime / 1000d, Finish)); // enqueue spell to be executed after cast time

            events.EnqueueEvent(new SpellEvent(Parameters.SpellInfo.Entry.CastTime / 1000d, Execute)); // enqueue spell to be executed after cast time

            status = SpellStatus.Casting;
            log.LogTrace($"Spell {Parameters.SpellInfo.Entry.Id} has started casting.");
            return true;
        }

        protected override bool _IsCasting()
        {
            return base._IsCasting() && status == SpellStatus.Casting;
        }
    }
}
