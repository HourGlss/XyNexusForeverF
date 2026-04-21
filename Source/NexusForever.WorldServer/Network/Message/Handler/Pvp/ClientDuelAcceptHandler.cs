using NexusForever.Game.Pvp;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Pvp;

namespace NexusForever.WorldServer.Network.Message.Handler.Pvp
{
    public class ClientDuelAcceptHandler : IMessageHandler<IWorldSession, ClientDuelAccept>
    {
        public void HandleMessage(IWorldSession session, ClientDuelAccept _)
        {
            DuelManager.Instance.HandleAccept(session.Player);
        }
    }
}
