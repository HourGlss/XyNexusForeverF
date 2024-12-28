using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Stat;

namespace NexusForever.Game.Entity.Stat
{
    public class EntityStatUpdateManager : IStatUpdateManager<IUnitEntity>
    {
        private readonly List<IStatUpdater> statUpdaters = [];
        private IUnitEntity entity;

        #region Dependency Injection

        private readonly IServiceProvider serviceProvider;

        public EntityStatUpdateManager(
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        #endregion

        public virtual void Initialise(IUnitEntity entity)
        {
            this.entity = entity;

            foreach (var statUpdater in serviceProvider.GetServices<IStatUpdater<IUnitEntity>>())
                Add(entity, statUpdater);
        }

        protected void Add<T>(T entity, IStatUpdater<T> statUpdater) where T : IUnitEntity
        {
            statUpdater.Initialise(entity);
            statUpdaters.Add(statUpdater);
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            if (!entity.IsAlive)
                return;

            foreach (IStatUpdater statUpdater in statUpdaters)
                statUpdater.Update(lastTick);
        }

        /// <summary>
        /// Invoked when a stat value changes.
        /// </summary>
        public void OnStatUpdate(IStatValue value, float previousValue)
        {
            if (!entity.IsAlive)
                return;

            foreach (IStatUpdater statUpdater in statUpdaters)
                statUpdater.OnStatUpdate(value, previousValue);
        }

        /// <summary>
        /// Invoked when the combat state changes.
        /// </summary>
        public void OnCombatStateUpdate(bool inCombat)
        {
            foreach (IStatUpdater statUpdater in statUpdaters)
                statUpdater.OnCombatStateUpdate(inCombat);
        }
    }
}
