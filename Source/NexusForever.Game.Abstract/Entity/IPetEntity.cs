using NexusForever.Game.Abstract.Entity.Creature;

namespace NexusForever.Game.Abstract.Entity
{
    public interface IPetEntity : IWorldEntity
    {
        uint OwnerGuid { get; }

        void Initialise(IPlayer owner, ICreatureInfo creatureInfo);
    }
}
