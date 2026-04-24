using NexusForever.Game.Static.Entity;

namespace NexusForever.Game.Entity
{
    internal static class PlayerResourceDefaults
    {
        internal const float FocusPoolFallback = 100f;
        internal const float FocusRecoveryFallback = 0.011f;

        public static IEnumerable<(Property Property, float Value)> GetBasePropertyFallbacks(Class @class, Func<Property, float> getProperty)
        {
            if (!UsesFocus(@class))
                yield break;

            if (getProperty(Property.BaseFocusPool) <= 0f)
                yield return (Property.BaseFocusPool, FocusPoolFallback);

            if (getProperty(Property.BaseFocusRecoveryInCombat) <= 0f)
                yield return (Property.BaseFocusRecoveryInCombat, FocusRecoveryFallback);

            if (getProperty(Property.BaseFocusRecoveryOutofCombat) <= 0f)
                yield return (Property.BaseFocusRecoveryOutofCombat, FocusRecoveryFallback);
        }

        public static IEnumerable<(NexusForever.Game.Static.Entity.Stat Stat, float Value)> GetMissingVitalDefaults(Class @class, Func<NexusForever.Game.Static.Entity.Stat, float?> getStat, Func<Property, float> getProperty)
        {
            if (UsesFocus(@class)
                && getStat(NexusForever.Game.Static.Entity.Stat.Focus) == null
                && getProperty(Property.BaseFocusPool) > 0f)
            {
                yield return (NexusForever.Game.Static.Entity.Stat.Focus, getProperty(Property.BaseFocusPool));
            }

            if (@class == Class.Medic
                && getStat(NexusForever.Game.Static.Entity.Stat.Resource1) == null
                && getProperty(Property.ResourceMax1) > 0f)
            {
                yield return (NexusForever.Game.Static.Entity.Stat.Resource1, getProperty(Property.ResourceMax1));
            }
        }

        public static bool UsesFocus(Class @class)
        {
            return @class is Class.Esper or Class.Medic or Class.Spellslinger;
        }
    }
}
