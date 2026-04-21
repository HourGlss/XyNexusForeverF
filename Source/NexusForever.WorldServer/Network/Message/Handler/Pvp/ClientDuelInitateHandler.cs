using NexusForever.Game.Pvp;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Pvp;

namespace NexusForever.WorldServer.Network.Message.Handler.Pvp
{
    public class ClientDuelInitateHandler : IMessageHandler<IWorldSession, ClientDuelInitate>
    {
        public void HandleMessage(IWorldSession session, ClientDuelInitate _)
        {
            DuelManager.Instance.HandleRequest(session.Player);
        }
    }
}
