using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.PVP;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.PvpFlag)]
    public class PrerequisiteCheckPvpFlag : BasePrerequisiteHandler, IPrerequisiteCheck
    {
        public PrerequisiteCheckPvpFlag(
            ILogger<BasePrerequisiteHandler> log)
            : base(log)
        {
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            IUnitEntity unit = GetEvaluationUnit(player, parameters);
            uint isFlagged = unit is IPlayer targetPlayer && targetPlayer.PvPFlags != PvPFlag.Disabled ? 1u : 0u;
            return MatchComparable(isFlagged, value, comparison, PrerequisiteType.PvpFlag);
        }
    }
}
