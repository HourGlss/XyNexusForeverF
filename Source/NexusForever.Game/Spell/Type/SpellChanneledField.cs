using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell.Type
{
    public class SpellChanneledField : Spell
    {
        public override CastMethod CastMethod => CastMethod.ChanneledField;

        public SpellChanneledField(
            ILogger<SpellChanneledField> log,
            ISpellTargetInfoCollection spellTargetInfoCollection,
            IGlobalSpellManager globalSpellManager)
            : base(log, spellTargetInfoCollection, globalSpellManager)
        {
        }
    }
}
