using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Abstract.Map.Search
{
    public interface ISearchCheckSpellTargetImplicit : ISearchCheck<IUnitEntity>
    {
        void Initialise(IUnitEntity searcher, Vector3 position, float radius, SpellTargetMechanicFlags targetMechanicFlags);
    }
}
