using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Stat;
using NexusForever.Game.Static.Entity;
using NexusForever.Shared.Game;

namespace NexusForever.Game.Entity.Stat.Player
{
    public class FocusStatUpdater : IStatUpdater<IPlayer>
    {
        private readonly UpdateTimer updateTimer = new(TimeSpan.FromSeconds(1f));

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
            updateTimer.Update(lastTick);
            if (!updateTimer.HasElapsed)
                return;

            if (player.Focus >= player.GetPropertyValue(Property.BaseFocusPool))
                return;

            updateTimer.Reset();

            float recoveryRate = player.GetPropertyValue(player.InCombat ? Property.BaseFocusRecoveryInCombat : Property.BaseFocusRecoveryOutofCombat);
            player.Focus += player.GetPropertyValue(Property.BaseFocusPool) * recoveryRate;
        }
    }
}
