using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.Stealth)]
    public class PrerequisiteCheckStealth : BasePrerequisiteHandler, IPrerequisiteCheck
    {
        public PrerequisiteCheckStealth(
            ILogger<BasePrerequisiteHandler> log)
            : base(log)
        {
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            IUnitEntity unit = GetEvaluationUnit(player, parameters);
            return MatchBoolean(unit.Stealthed, comparison, PrerequisiteType.Stealth);
        }
    }
}
