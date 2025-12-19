using NexusForever.Game.Abstract.Entity;

namespace NexusForever.Script.Template
{
    public interface ICanSeeMeScript
    {
        /// <summary>
        /// Determines whether the specified <see cref="IGridEntity"/> can see this entity.
        /// </summary>
        bool CanSeeMe(IGridEntity entity);
    }
}
