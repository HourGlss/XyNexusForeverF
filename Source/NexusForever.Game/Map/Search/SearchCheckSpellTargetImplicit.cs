using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Search;
using NexusForever.Game.Static.Reputation;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Map.Search
{
    public class SearchCheckSpellTargetImplicit : ISearchCheckSpellTargetImplicit
    {
        private Vector3 vector;
        private float radius;
        private IUnitEntity searcher;
        private SpellTargetMechanicFlags targetMechanicFlags;

        public void Initialise(IUnitEntity searcher, Vector3 position, float radius, SpellTargetMechanicFlags targetMechanicFlags)
        {
            vector                   = position;
            this.radius              = radius;
            this.searcher            = searcher;
            this.targetMechanicFlags = targetMechanicFlags;
        }

        /// <summary>
        /// Check if <see cref="IUnitEntity"/> should be included in the search.
        /// </summary>
        public virtual bool CheckEntity(IUnitEntity entity)
        {
            // TODO: this should probably be split into multiple as this method might get too large
            if (!entity.IsAlive)
                return false;

            if (radius > 0 && Vector3.Distance(vector, entity.Position) > radius)
                return false;

            // TODO: Check Angle

            if (targetMechanicFlags.HasFlag(SpellTargetMechanicFlags.IsPlayer) && entity is not IPlayer)
                return false;

            // Check Target Flags
            if (entity.Faction1 == 0 && entity.Faction2 == 0) // Unable to evaluate units with no factions specified, unless this means Neutral?
                return false;

            if (targetMechanicFlags.HasFlag(SpellTargetMechanicFlags.IsEnemy))
            {
                // TODO: handle other things like "Is Immune", "Is Player and PvP Enabled"

                if (searcher.GetDispositionTo(entity.Faction1, true) > Disposition.Neutral)
                    return false;
            }

            if (targetMechanicFlags.HasFlag(SpellTargetMechanicFlags.IsFriendly)
                && searcher.GetDispositionTo(entity.Faction1, true) < Disposition.Neutral)
                return false;

            return true;
        }
    }
}
