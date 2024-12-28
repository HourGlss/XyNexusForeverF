using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell.Target.Implicit;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Target;

namespace NexusForever.Game.Spell.Target.Implicit
{
    public class SpellTargetImplicit : ISpellTargetImplicit
    {
        public IUnitEntity Entity { get; }
        public SpellEffectTargetFlags Flags { get; set; } = SpellEffectTargetFlags.ImplicitTarget;
        public SpellTargetImplicitSelectionResult? Result { get; set; }

        /// <summary>
        /// Distance between the caster and implicit target.
        /// </summary>
        public float Distance { get; set; }

        public SpellTargetImplicit(IUnitEntity entity, float distance)
        {
            Entity   = entity;
            Distance = distance;
        }
    }
}
