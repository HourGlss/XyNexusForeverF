using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.Game.Static.Reputation;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.Faction)]
    [PrerequisiteCheck(PrerequisiteType.Faction128)]
    [PrerequisiteCheck(PrerequisiteType.Faction243)]
    public class PrerequisiteCheckFaction : BasePrerequisiteHandler, IPrerequisiteCheck
    {
        public PrerequisiteCheckFaction(
            ILogger<BasePrerequisiteHandler> log)
            : base(log)
        {
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            IUnitEntity unit = GetEvaluationUnit(player, parameters);
            return MatchEnum(unit.Faction1, (Faction)value, comparison, PrerequisiteType.Faction);
        }
    }
}
