using System;

namespace NexusForever.Shared
{
    public static class BuildInfo
    {
        public const string Milestone = "XYF-1.2";

        public static string WithMilestone(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return Milestone;

            return HasMilestonePrefix(message)
                ? message
                : $"{Milestone} {message}";
        }

        private static bool HasMilestonePrefix(string message)
        {
            return string.Equals(message, Milestone, StringComparison.Ordinal)
                || message.StartsWith($"{Milestone} ", StringComparison.Ordinal);
        }
    }
}
