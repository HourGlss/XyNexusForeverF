using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Map;

namespace NexusForever.Script.Template.Event
{
    public interface IEntitySummonEvent : IScriptEvent
    {
        void Initialise(IEntitySummonFactory factory, ICreatureInfo creatureInfo, Vector3 position, Vector3 rotation, OnAddDelegate add = null);
    }
}
