namespace NexusForever.Game.Abstract.Entity.Stat
{
    public interface IStatUpdateManager<in T> : IStatUpdateManager where T : IUnitEntity
    {
        void Initialise(T entity);
    }
}
