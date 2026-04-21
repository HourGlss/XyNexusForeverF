using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.DeadState)]
    public class PrerequisiteCheckDeadState : BasePrerequisiteHandler, IPrerequisiteCheck
    {
        public PrerequisiteCheckDeadState(
            ILogger<BasePrerequisiteHandler> log)
            : base(log)
        {
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            EntityDeathState? deathState = player.DeathState;
            if (deathState == null)
            {
                return comparison switch
                {
                    PrerequisiteComparison.Equal              => false,
                    PrerequisiteComparison.NotEqual           => true,
                    PrerequisiteComparison.GreaterThan        => false,
                    PrerequisiteComparison.GreaterThanOrEqual => false,
                    PrerequisiteComparison.LessThan           => false,
                    PrerequisiteComparison.LessThanOrEqual    => false,
                    _                                         => UnhandledComparison(comparison, PrerequisiteType.DeadState)
                };
            }

            return MatchEnum(deathState.Value, (EntityDeathState)value, comparison, PrerequisiteType.DeadState);
        }
    }
}
