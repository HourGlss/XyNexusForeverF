using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.Unknown64)]
    public class PrerequisiteCheckUnknown64 : BasePrerequisiteHandler, IPrerequisiteCheck
    {
        public PrerequisiteCheckUnknown64(
            ILogger<BasePrerequisiteHandler> log)
            : base(log)
        {
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            if (!TryGetResourceValue(player, objectId, out float currentValue))
            {
                log.LogWarning("Unhandled Unknown64 prerequisite object {ObjectId} for player class {Class}.", objectId, player.Class);
                return false;
            }

            return MatchComparable(currentValue, (float)value, comparison, PrerequisiteType.Unknown64);
        }

        private static bool TryGetResourceValue(IPlayer player, uint objectId, out float currentValue)
        {
            switch (objectId)
            {
                case 1:
                    currentValue = player.Class == Class.Spellslinger
                        ? player.Resource4
                        : player.Resource1;
                    return true;
                case 4:
                    currentValue = player.Resource4;
                    return true;
                default:
                    currentValue = 0f;
                    return false;
            }
        }
    }
}
