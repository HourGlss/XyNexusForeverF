using NexusForever.Game.Abstract.Entity;

namespace NexusForever.Game.Entity
{
    public class EntityTemplateStat : IEntityTemplateStat
    {
        public Static.Entity.Stat Stat { get; set; }
        public float Value { get; set; }
    }
}
