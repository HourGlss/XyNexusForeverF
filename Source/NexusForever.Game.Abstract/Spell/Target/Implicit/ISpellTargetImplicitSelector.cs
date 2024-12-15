using NexusForever.Game.Abstract.Entity;

namespace NexusForever.Game.Abstract.Spell.Target.Implicit
{
    public interface ISpellTargetImplicitSelector
    {
        void Initialise(IUnitEntity caster, ISpellParameters parameters);

        void SelectTargets(List<ISpellTargetImplicit> implicitTargets);
    }
}
