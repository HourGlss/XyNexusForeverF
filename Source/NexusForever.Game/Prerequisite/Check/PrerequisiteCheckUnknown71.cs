using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.Unknown71)]
    public class PrerequisiteCheckUnknown71 : BasePrerequisiteHandler, IPrerequisiteCheck
    {
        public PrerequisiteCheckUnknown71(
            ILogger<BasePrerequisiteHandler> log)
            : base(log)
        {
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            if (!TryGetVitalPercent(player, objectId, out float currentPercent))
            {
                log.LogWarning("Unhandled Unknown71 prerequisite object {ObjectId}.", objectId);
                return false;
            }

            return MatchComparable(currentPercent, (float)value, comparison, PrerequisiteType.Unknown71);
        }

        private static bool TryGetVitalPercent(IPlayer player, uint objectId, out float currentPercent)
        {
            Vital vital = (Vital)objectId;
            float maxValue = vital switch
            {
                Vital.Health => player.GetPropertyValue(Property.BaseHealth),
                Vital.ShieldCapacity => player.GetPropertyValue(Property.ShieldCapacityMax),
                Vital.Endurance => player.GetPropertyValue(Property.ResourceMax0),
                Vital.Resource1 or Vital.KineticEnergy or Vital.Volatility or Vital.Actuator or Vital.Actuator2
                    => player.GetPropertyValue(Property.ResourceMax1),
                Vital.Resource2 => player.GetPropertyValue(Property.ResourceMax2),
                Vital.Resource3 or Vital.SuitPower => player.GetPropertyValue(Property.ResourceMax3),
                Vital.Resource4 or Vital.SpellSurge => player.GetPropertyValue(Property.ResourceMax4),
                Vital.Focus => player.GetPropertyValue(Property.BaseFocusPool),
                _ => 0f
            };

            if (maxValue <= 0f)
            {
                currentPercent = 0f;
                return false;
            }

            currentPercent = player.GetVitalValue(vital) / maxValue * 100f;
            return true;
        }
    }
}
