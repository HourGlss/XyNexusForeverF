namespace NexusForever.Game.Abstract.Entity.Stat
{
    public interface IStatUpdater<T> : IStatUpdater
        where T : IUnitEntity
    {
        void Initialise(T entity);
    }
}
