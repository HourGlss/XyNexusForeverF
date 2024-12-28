using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Abstract.Spell.Target
{
    public interface ISpellTarget
    {
        IUnitEntity Entity { get; }
        SpellEffectTargetFlags Flags { get; set; }
    }
}
