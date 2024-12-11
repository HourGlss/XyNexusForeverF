using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Stat;
using NexusForever.Game.Static.Entity;
using NexusForever.Shared.Game;

namespace NexusForever.Game.Entity.Stat.Entity
{
    public class SheildStatUpdater : IStatUpdater<IUnitEntity>
    {
        private readonly UpdateTimer updateTimer = new(TimeSpan.FromSeconds(0.25f));

        private IUnitEntity entity;

        public void Initialise(IUnitEntity entity)
        {
            this.entity = entity;
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

            if (entity.Shield < entity.MaxShieldCapacity)
                entity.Shield += (uint)(entity.MaxShieldCapacity * entity.GetPropertyValue(Property.ShieldRegenPct) * updateTimer.Duration);
        }

        /// <summary>
        /// Invoked when the combat state changes.
        /// </summary>
        public void OnCombatStateUpdate(bool inCombat)
        {
            updateTimer.Reset(!inCombat);
        }
    }
}
