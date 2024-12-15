namespace NexusForever.Game.Abstract.Spell.Target.Implicit
{
    public interface ISpellTargetImplicitFilter
    {
        void Filter(List<ISpellTargetImplicit> implicitTargets);
    }
}
