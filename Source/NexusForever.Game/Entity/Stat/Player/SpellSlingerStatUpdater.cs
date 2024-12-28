using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Stat;
using NexusForever.Shared.Game;

namespace NexusForever.Game.Entity.Stat.Player
{
    public class SpellSlingerStatUpdater : IStatUpdater<IPlayer>
    {
        private UpdateTimer updateTimer = new(TimeSpan.FromSeconds(1));

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

            if (player.Resource4 < player.GetPropertyValue(Static.Entity.Property.ResourceMax4))
                player.Resource4 += 4;
        }
    }
}
