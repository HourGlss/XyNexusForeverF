using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.Game.Static.Reputation;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.Reputation)]
    public class PrerequisiteCheckReputation : BasePrerequisiteHandler, IPrerequisiteCheck
    {
        public PrerequisiteCheckReputation(
            ILogger<BasePrerequisiteHandler> log)
            : base(log)
        {
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            Disposition disposition = player.GetDispositionTo(player.Faction1);
            return MatchEnum(disposition, (Disposition)value, comparison, PrerequisiteType.Reputation);
        }
    }
}
