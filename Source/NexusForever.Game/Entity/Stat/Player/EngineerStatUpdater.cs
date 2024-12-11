using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Stat;
using NexusForever.Shared.Game;

namespace NexusForever.Game.Entity.Stat.Player
{
    public class EngineerStatUpdater : IStatUpdater<IPlayer>
    {
        private readonly UpdateTimer outOfCombatTimer = new(TimeSpan.FromSeconds(3f));
        private readonly UpdateTimer decayTimer = new(TimeSpan.FromSeconds(0.5f));

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
            outOfCombatTimer.Update(lastTick);
            if (!outOfCombatTimer.HasElapsed)
                return;

            decayTimer.Update(lastTick);
            if (!decayTimer.HasElapsed)
                return;

            decayTimer.Reset();

            if (player.Resource1 > 0f)
                player.Resource1 -= 10f;
        }

        /// <summary>
        /// Invoked when the combat state changes.
        /// </summary>
        public void OnCombatStateUpdate(bool inCombat)
        {
            outOfCombatTimer.Reset(!inCombat);
        }
    }
}
