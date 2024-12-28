using NexusForever.Shared;

namespace NexusForever.Game.Abstract.Entity.Stat
{
    public interface IStatUpdateManager : IUpdate
    {
        /// <summary>
        /// Invoked when a stat value changes.
        /// </summary>
        void OnStatUpdate(IStatValue value, float previousValue);

        /// <summary>
        /// Invoked when the combat state changes.
        /// </summary>
        void OnCombatStateUpdate(bool inCombat);
    }
}
