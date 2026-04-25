using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.InSubZone)]
    public class PrerequisiteCheckInSubZone : BasePrerequisiteHandler, IPrerequisiteCheck
    {
        public PrerequisiteCheckInSubZone(
            ILogger<BasePrerequisiteHandler> log)
            : base(log)
        {
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            IUnitEntity unit = GetEvaluationUnit(player, parameters);
            uint zoneId = unit.Zone?.Id ?? 0u;
            return MatchComparable(zoneId, value, comparison, PrerequisiteType.InSubZone);
        }
    }
}
