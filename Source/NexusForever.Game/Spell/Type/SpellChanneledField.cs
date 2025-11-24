using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Abstract.Spell.Validator;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell.Type
{
    public class SpellChanneledField : Spell
    {
        public override CastMethod CastMethod => CastMethod.ChanneledField;

        public SpellChanneledField(
            ILogger<SpellChanneledField> log,
            ISpellTargetInfoCollection spellTargetInfoCollection,
            IGlobalSpellManager globalSpellManager,
            ICastResultValidatorManager castResultValidatorManager,
            IDisableManager disableManager)
            : base(log, spellTargetInfoCollection, globalSpellManager, castResultValidatorManager, disableManager)
        {
        }
    }
}
