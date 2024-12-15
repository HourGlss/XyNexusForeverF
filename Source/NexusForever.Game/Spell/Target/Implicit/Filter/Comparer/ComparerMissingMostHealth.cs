using NexusForever.Game.Abstract.Spell.Target.Implicit;

namespace NexusForever.Game.Spell.Target.Implicit.Filter.Comparer
{
    public class ComparerMissingMostHealth : IComparer<ISpellTargetImplicit>
    {
        public int Compare(ISpellTargetImplicit x, ISpellTargetImplicit y)
        {
            if (x == null || y == null)
                return 0;

            if (x.Entity.MaxHealth - x.Entity.Health < y.Entity.MaxHealth - y.Entity.Health)
                return 1;

            if (x.Entity.MaxHealth - x.Entity.Health > y.Entity.MaxHealth - y.Entity.Health)
                return -1;

            return 0;
        }
    }
}
