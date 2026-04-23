using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Search;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Target.Implicit;
using NexusForever.Game.Abstract.Spell.Target.Implicit.Filter;
using NexusForever.Game.Static.Spell.Target;
using NexusForever.Shared;

namespace NexusForever.Game.Spell.Target.Implicit.Filter
{
    public class SpellTargetImplicitTelegraphFilter : ISpellTargetImplicitTelegraphFilter
    {
        private IReadOnlyList<ITelegraph> telegraphs;
        private IUnitEntity caster;

        #region Dependency Injection

        private readonly IFactory<ISearchCheckTelegraph> searchCheckTelegraphFactory;

        public SpellTargetImplicitTelegraphFilter(
            IFactory<ISearchCheckTelegraph> searchCheckTelegraphFactory)
        {
            this.searchCheckTelegraphFactory = searchCheckTelegraphFactory;
        }

        #endregion

        public void Initialise(ITelegraph telegraph, IUnitEntity caster)
        {
            Initialise([telegraph], caster);
        }

        public void Initialise(IEnumerable<ITelegraph> telegraphs, IUnitEntity caster)
        {
            if (this.telegraphs != null)
                throw new InvalidOperationException("SpellTargetImplicitTelegraphFilter has already been initialised.");

            this.telegraphs = telegraphs.ToList();
            this.caster     = caster;
        }

        public void Filter(List<ISpellTargetImplicit> targets)
        {
            if (telegraphs.Count == 0)
                return;

            foreach (ISpellTargetImplicit target in targets)
            {
                if (target.Result != null)
                    continue;

                if (!IsInsideAnyTelegraph(target.Entity))
                    target.Result = SpellTargetImplicitSelectionResult.OutsideTelegraph;
            }
        }

        private bool IsInsideAnyTelegraph(IUnitEntity entity)
        {
            foreach (ITelegraph telegraph in telegraphs)
            {
                ISearchCheckTelegraph searchCheckTelegraph = searchCheckTelegraphFactory.Resolve();
                searchCheckTelegraph.Initialise(telegraph, caster);

                if (searchCheckTelegraph.CheckEntity(entity))
                    return true;
            }

            return false;
        }
    }
}
