using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Stat;
using NexusForever.Game.Static.Entity;
using NexusForever.Shared.Game;

namespace NexusForever.Game.Entity.Stat.Player
{
    public class MedicStatUpdater : IStatUpdater<IPlayer>
    {
        private readonly UpdateTimer outOfCombatTimer = new(TimeSpan.FromSeconds(3f));

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

            outOfCombatTimer.Reset(false);

            player.Resource1 = player.GetPropertyValue(Property.ResourceMax1);
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
