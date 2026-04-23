using NexusForever.Game.Abstract.Entity;

namespace NexusForever.Game.Abstract.Spell.Target.Implicit.Filter
{
    public interface ISpellTargetImplicitTelegraphFilter : ISpellTargetImplicitFilter
    {
        void Initialise(ITelegraph telegraph, IUnitEntity caster);
        void Initialise(IEnumerable<ITelegraph> telegraphs, IUnitEntity caster);
    }
}
