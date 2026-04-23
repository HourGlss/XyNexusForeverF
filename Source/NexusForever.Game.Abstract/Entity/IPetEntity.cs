using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Static.Pet;

namespace NexusForever.Game.Abstract.Entity
{
    public interface IPetEntity : IUnitEntity
    {
        uint OwnerGuid { get; }
        bool IsCombatPet { get; }
        uint SummoningSpell4Id { get; }
        PetStance Stance { get; }

        void Initialise(IPlayer owner, ICreatureInfo creatureInfo);
        void InitialiseCombat(IPlayer owner, ICreatureInfo creatureInfo, uint summoningSpell4Id);
        void SetStance(PetStance stance);
    }
}
