using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;

namespace NexusForever.Game.Abstract.Map.Search
{
    public interface ISearchCheckTelegraph : ISearchCheck<IUnitEntity>
    {
        void Initialise(ITelegraph telegraph, IUnitEntity caster);
    }
}
