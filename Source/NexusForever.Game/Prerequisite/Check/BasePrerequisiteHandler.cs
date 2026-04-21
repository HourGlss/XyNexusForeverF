using Microsoft.Extensions.Logging;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    public abstract class BasePrerequisiteHandler
    {
        protected readonly ILogger<BasePrerequisiteHandler> log;

        protected BasePrerequisiteHandler(ILogger<BasePrerequisiteHandler> log)
        {
            this.log = log;
        }

        protected bool MatchEnum<T>(T entityValue, T value, PrerequisiteComparison comparison, PrerequisiteType type)
            where T : struct, Enum
        {
            return MatchComparable(Convert.ToInt64(entityValue), Convert.ToInt64(value), comparison, type);
        }

        protected bool MatchComparable<T>(T entityValue, T value, PrerequisiteComparison comparison, PrerequisiteType type)
            where T : IComparable<T>
        {
            int result = entityValue.CompareTo(value);
            return comparison switch
            {
                PrerequisiteComparison.Equal              => result == 0,
                PrerequisiteComparison.NotEqual           => result != 0,
                PrerequisiteComparison.GreaterThan        => result > 0,
                PrerequisiteComparison.GreaterThanOrEqual => result >= 0,
                PrerequisiteComparison.LessThan           => result < 0,
                PrerequisiteComparison.LessThanOrEqual    => result <= 0,
                _                                         => UnhandledComparison(comparison, type)
            };
        }

        protected bool MatchBoolean(bool entityValue, PrerequisiteComparison comparison, PrerequisiteType type)
        {
            return comparison switch
            {
                PrerequisiteComparison.Equal    => entityValue,
                PrerequisiteComparison.NotEqual => !entityValue,
                _                               => UnhandledComparison(comparison, type)
            };
        }

        protected bool UnhandledComparison(PrerequisiteComparison comparison, PrerequisiteType type)
        {
            log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {type}!");
            return false;
        }
    }
}
