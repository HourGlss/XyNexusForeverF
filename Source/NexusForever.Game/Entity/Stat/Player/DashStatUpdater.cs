using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Stat;
using NexusForever.Game.Static.Entity;
using NexusForever.Shared.Game;

namespace NexusForever.Game.Entity.Stat.Player
{
    public class DashStatUpdater : IStatUpdater<IPlayer>
    {
        private readonly UpdateTimer updateTimer = new(TimeSpan.FromSeconds(0.25f));

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

            updateTimer.Reset();

            if (player.Dash >= player.GetPropertyValue(Property.ResourceMax7))
                return;

            float dashRegenAmount = player.GetPropertyValue(Property.ResourceMax7) * player.GetPropertyValue(Property.ResourceRegenMultiplier7);
            player.Dash += dashRegenAmount;
        }
    }
}
