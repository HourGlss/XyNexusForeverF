using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Stat;
using NexusForever.Game.Static.Entity;
using NexusForever.Shared.Game;

namespace NexusForever.Game.Entity.Stat.Player
{
    public class StalkerStatUpdater : IStatUpdater<IPlayer>
    {
        private readonly UpdateTimer updateTimer = new(TimeSpan.FromSeconds(0.5f));

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

            if (player.Resource3 >= player.GetPropertyValue(Property.ResourceMax3))
                return;

            float resource3RegenAmount = player.GetPropertyValue(Property.ResourceMax3) * player.GetPropertyValue(Property.ResourceRegenMultiplier3);
            player.Resource3 += resource3RegenAmount;
        }
    }
}
