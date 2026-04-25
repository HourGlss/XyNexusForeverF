using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.Level)]
    [PrerequisiteCheck(PrerequisiteType.Level131)]
    [PrerequisiteCheck(PrerequisiteType.TrueLevel)]
    public class PrerequisiteCheckLevel : BasePrerequisiteHandler, IPrerequisiteCheck
    {
        public PrerequisiteCheckLevel(
            ILogger<BasePrerequisiteHandler> log)
            : base(log)
        {
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            IUnitEntity unit = GetEvaluationUnit(player, parameters);
            return MatchComparable(unit.Level, value, comparison, PrerequisiteType.Level);
        }
    }
}
