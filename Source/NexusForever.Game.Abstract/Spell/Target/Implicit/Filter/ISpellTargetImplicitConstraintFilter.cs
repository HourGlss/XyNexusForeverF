using NexusForever.GameTable.Model;

namespace NexusForever.Game.Abstract.Spell.Target.Implicit.Filter
{
    public interface ISpellTargetImplicitConstraintFilter : ISpellTargetImplicitFilter
    {
        void Initialise(Spell4AoeTargetConstraintsEntry constraints);
    }
}
