using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.Game.Static.Reputation;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.BaseFaction)]
    public class PrerequisiteCheckBaseFaction : BasePrerequisiteHandler, IPrerequisiteCheck
    {
        public PrerequisiteCheckBaseFaction(
            ILogger<BasePrerequisiteHandler> log)
            : base(log)
        {
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            IUnitEntity unit = GetEvaluationUnit(player, parameters);
            return MatchEnum(unit.Faction1, (Faction)value, comparison, PrerequisiteType.BaseFaction);
        }
    }
}
