using NexusForever.Game.Abstract.Spell.Target.Implicit;
using NexusForever.Game.Abstract.Spell.Target.Implicit.Filter;
using NexusForever.Game.Spell.Target.Implicit.Filter.Comparer;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Target;
using NexusForever.GameTable.Model;
using NexusForever.Shared;

namespace NexusForever.Game.Spell.Target.Implicit.Filter
{
    public class SpellTargetImplicitConstraintFilter : ISpellTargetImplicitConstraintFilter
    {
        private Spell4AoeTargetConstraintsEntry constraints;

        public void Initialise(Spell4AoeTargetConstraintsEntry constraints)
        {
            this.constraints = constraints;
        }

        public void Filter(List<ISpellTargetImplicit> targets)
        {
            if (constraints == null)
                return;

            uint count = 0u;
            foreach (ISpellTargetImplicit target in targets)
            {
                // no point evaluating if the target is not inside the telegraph
                if (target.Result != null)
                    continue;

                SpellTargetImplicitSelectionResult? result = FilterImplicitTarget(target, count++);
                if (result.HasValue)
                    target.Result = result.Value;
            }

            OrderForSelectionType(targets);
        }

        private SpellTargetImplicitSelectionResult? FilterImplicitTarget(ISpellTargetImplicit target, uint count)
        {
            if (constraints.TargetCount > 0 && count >= constraints.TargetCount)
                return SpellTargetImplicitSelectionResult.CountConstraintFailed;

            // TODO
            if (constraints.Angle > 0)
            {
            }

            if (constraints.MaxRange > 0 && target.Distance > constraints.MaxRange)
                return SpellTargetImplicitSelectionResult.DistanceConstraintFailed;

            if (constraints.MinRange > 0 && target.Distance < constraints.MinRange)
                return SpellTargetImplicitSelectionResult.DistanceConstraintFailed;

            return null;
        }

        private void OrderForSelectionType(List<ISpellTargetImplicit> targets)
        {
            switch (constraints.TargetSelection)
            {
                case AoeSelectionType.Closest:
                    targets.Sort(new ComparerClosest());
                    break;
                case AoeSelectionType.Furthest:
                    targets.Sort(new ComparerFurthest());
                    break;
                case AoeSelectionType.Random:
                    targets.Shuffle();
                    break;
                case AoeSelectionType.LowestAbsoluteHealth:
                    targets.Sort(new ComparerLowestAbsoluteHealth());
                    break;
                case AoeSelectionType.MissingMostHealth:
                    targets.Sort(new ComparerMissingMostHealth());
                    break;
                case AoeSelectionType.None:
                default:
                    // Do nothing. Leave it ordered as it was evaluated.
                    break;
            }
        }
    }
}
