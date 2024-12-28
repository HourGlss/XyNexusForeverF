using NexusForever.Game.Abstract.Spell.Target.Implicit;

namespace NexusForever.Game.Spell.Target.Implicit.Filter.Comparer
{
    public class ComparerFurthest : IComparer<ISpellTargetImplicit>
    {
        public int Compare(ISpellTargetImplicit x, ISpellTargetImplicit y)
        {
            if (x == null || y == null)
                return 0;

            if (x.Distance < y.Distance)
                return 1;

            if (x.Distance > y.Distance)
                return -1;

            return 0;
        }
    }
}
