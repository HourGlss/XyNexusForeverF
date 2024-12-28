using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Search;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Map.Search
{
    public class SearchCheckTelegraph : ISearchCheckTelegraph
    {
        private ITelegraph telegraph;
        private IUnitEntity caster;

        public void Initialise(ITelegraph telegraph, IUnitEntity caster)
        {
            if (this.telegraph != null)
                throw new InvalidOperationException("SearchCheckTelegraph has already been initialised.");

            this.telegraph = telegraph;
            this.caster    = caster;
        }

        public bool CheckEntity(IUnitEntity entity)
        {
            if (telegraph.TelegraphTargetTypeFlags.HasFlag(TelegraphTargetTypeFlags.Self) && entity != caster)
                return false;

            if (telegraph.TelegraphTargetTypeFlags.HasFlag(TelegraphTargetTypeFlags.Other) && entity == caster)
                return false;

            return telegraph.InsideTelegraph(entity.Position, entity.HitRadius);
        }
    }
}
