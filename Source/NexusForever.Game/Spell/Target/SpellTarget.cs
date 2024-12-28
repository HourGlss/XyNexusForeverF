using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell.Target
{
    public class SpellTarget : ISpellTarget
    {
        public IUnitEntity Entity { get; }
        public SpellEffectTargetFlags Flags { get; set; }

        public SpellTarget(IUnitEntity entity, SpellEffectTargetFlags flags)
        {
            Entity = entity;
            Flags  = flags;
        }
    }
}
