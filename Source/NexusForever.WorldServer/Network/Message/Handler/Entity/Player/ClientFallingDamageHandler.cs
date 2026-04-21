using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler.Entity.Player
{
    public class ClientFallingDamageHandler : IMessageHandler<IWorldSession, ClientFallingDamage>
    {
        public void HandleMessage(IWorldSession session, ClientFallingDamage fallingDamage)
        {
            session.Player.TakeFallingDamage(fallingDamage.HealthPercent);
        }
    }
}
