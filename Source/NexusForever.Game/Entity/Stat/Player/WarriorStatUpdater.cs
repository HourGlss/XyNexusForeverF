using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Stat;
using NexusForever.Shared.Game;

namespace NexusForever.Game.Entity.Stat.Player
{
    public class WarriorStatUpdater : IStatUpdater<IPlayer>
    {
        private readonly UpdateTimer builderTimer = new(TimeSpan.FromSeconds(1.5f));
        private readonly UpdateTimer decayTimer = new(TimeSpan.FromSeconds(1f));

        private IPlayer player;

        public void Initialise(IPlayer entity)
        {
            player = entity;
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            builderTimer.Update(lastTick);
            if (builderTimer.IsTicking && !builderTimer.HasElapsed)
                return;

            decayTimer.Update(lastTick);
            if (!decayTimer.HasElapsed)
                return;

            decayTimer.Reset();

            if (player.Resource1 >= 0)
                player.Resource1 -= 150f;
        }

        /// <summary>
        /// Invoked when a stat value changes.
        /// </summary>
        public void OnStatUpdate(IStatValue value, float previousValue)
        {
            if (value.Stat != Static.Entity.Stat.Resource1)
                return;

            if (value.Value > previousValue)
                builderTimer.Reset();
        }

        /// <summary>
        /// Invoked when the combat state changes.
        /// </summary>
        public void OnCombatStateUpdate(bool inCombat)
        {
            builderTimer.Reset(inCombat);
        }
    }
}
