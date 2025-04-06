using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Map;

namespace NexusForever.Script.Template.Event
{
    public class EntitySummonEvent : IEntitySummonEvent
    {
        private IEntitySummonFactory factory;
        private ICreatureInfo creatureInfo;
        private Vector3 position;
        private Vector3 rotation;
        private OnAddDelegate add;

        public void Initialise(IEntitySummonFactory factory, ICreatureInfo creatureInfo, Vector3 position, Vector3 rotation, OnAddDelegate add = null)
        {
            this.factory      = factory;
            this.creatureInfo = creatureInfo;
            this.position     = position;
            this.rotation     = rotation;
            this.add          = add;
        }

        public void Invoke()
        {
            factory.Summon(creatureInfo, position, rotation, add);
        }
    }
}
