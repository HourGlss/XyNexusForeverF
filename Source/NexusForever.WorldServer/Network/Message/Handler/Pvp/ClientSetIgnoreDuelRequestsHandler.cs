using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Pvp;

namespace NexusForever.WorldServer.Network.Message.Handler.Pvp
{
    public class ClientSetIgnoreDuelRequestsHandler : IMessageHandler<IWorldSession, ClientSetIgnoreDuelRequests>
    {
        public void HandleMessage(IWorldSession session, ClientSetIgnoreDuelRequests setIgnoreDuelRequests)
        {
            session.Player.IgnoreDuelRequests = setIgnoreDuelRequests.Ignore;
        }
    }
}
