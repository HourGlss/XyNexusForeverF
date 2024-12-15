using NexusForever.Game.Static.Spell.Target;

namespace NexusForever.Game.Abstract.Spell.Target.Implicit
{
    public interface ISpellTargetImplicit : ISpellTarget
    {
        SpellTargetImplicitSelectionResult? Result { get; set; }

        /// <summary>
        /// Distance between the caster and implicit target.
        /// </summary>
        float Distance { get; set; }
    }
}
