using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.IsCreature)]
    public class PrerequisiteCheckIsCreature : BasePrerequisiteHandler, IPrerequisiteCheck
    {
        public PrerequisiteCheckIsCreature(
            ILogger<BasePrerequisiteHandler> log)
            : base(log)
        {
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            IUnitEntity unit = GetEvaluationUnit(player, parameters);
            bool isCreature = unit is not IPlayer && unit.Type != EntityType.Player;
            if (value != 0u)
                isCreature &= unit.CreatureId == value;

            return MatchBoolean(isCreature, comparison, PrerequisiteType.IsCreature);
        }
    }

    [PrerequisiteCheck(PrerequisiteType.IsPlayer)]
    public class PrerequisiteCheckIsPlayer : BasePrerequisiteHandler, IPrerequisiteCheck
    {
        public PrerequisiteCheckIsPlayer(
            ILogger<BasePrerequisiteHandler> log)
            : base(log)
        {
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            IUnitEntity unit = GetEvaluationUnit(player, parameters);
            bool isPlayer = unit.Type == EntityType.Player || unit is IPlayer;
            return MatchBoolean(isPlayer, comparison, PrerequisiteType.IsPlayer);
        }
    }
}
