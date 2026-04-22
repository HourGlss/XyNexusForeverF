using System;
using System.Linq;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Pet;
using NexusForever.Network;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Pet;

namespace NexusForever.WorldServer.Network.Message.Handler.Pet
{
    public class ClientPetSetStanceHandler : IMessageHandler<IWorldSession, ClientPetSetStance>
    {
        public void HandleMessage(IWorldSession session, ClientPetSetStance petSetStance)
        {
            if (!Enum.IsDefined(petSetStance.Stance))
                throw new InvalidPacketValueException();

            if (petSetStance.PetUnitId == 0u)
            {
                SetAllPetStances(session.Player, petSetStance.Stance);
                return;
            }

            IWorldEntity entity = session.Player.GetVisible<IWorldEntity>(petSetStance.PetUnitId);
            if (entity is not IPetEntity pet || !pet.IsCombatPet || pet.OwnerGuid != session.Player.Guid)
                throw new InvalidPacketValueException();

            pet.SetStance(petSetStance.Stance);
        }

        private static void SetAllPetStances(IPlayer player, PetStance stance)
        {
            foreach (IPetEntity pet in player.SummonFactory.GetSummons<IPetEntity>().Where(p => p.IsCombatPet))
                pet.SetStance(stance);
        }
    }
}
