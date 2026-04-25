using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.Health)]
    public class PrerequisiteCheckHealth : BasePrerequisiteHandler, IPrerequisiteCheck
    {
        public PrerequisiteCheckHealth(
            ILogger<BasePrerequisiteHandler> log)
            : base(log)
        {
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            IUnitEntity unit = GetEvaluationUnit(player, parameters);
            float currentPercent = GetPercent(unit.Health, unit.MaxHealth);
            return MatchComparable(currentPercent, (float)value, comparison, PrerequisiteType.Health);
        }

        private static float GetPercent(uint current, uint max)
        {
            return max == 0u ? 0f : current / (float)max * 100f;
        }
    }

    [PrerequisiteCheck(PrerequisiteType.HealthRequirement)]
    public class PrerequisiteCheckHealthRequirement : BasePrerequisiteHandler, IPrerequisiteCheck
    {
        public PrerequisiteCheckHealthRequirement(
            ILogger<BasePrerequisiteHandler> log)
            : base(log)
        {
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            IUnitEntity unit = GetEvaluationUnit(player, parameters);
            return MatchComparable(unit.Health, value, comparison, PrerequisiteType.HealthRequirement);
        }
    }

    [PrerequisiteCheck(PrerequisiteType.Shield215)]
    public class PrerequisiteCheckShield : BasePrerequisiteHandler, IPrerequisiteCheck
    {
        public PrerequisiteCheckShield(
            ILogger<BasePrerequisiteHandler> log)
            : base(log)
        {
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            IUnitEntity unit = GetEvaluationUnit(player, parameters);
            float currentPercent = GetPercent(unit.Shield, unit.MaxShieldCapacity);
            return MatchComparable(currentPercent, (float)value, comparison, PrerequisiteType.Shield215);
        }

        private static float GetPercent(uint current, uint max)
        {
            return max == 0u ? 0f : current / (float)max * 100f;
        }
    }
}
