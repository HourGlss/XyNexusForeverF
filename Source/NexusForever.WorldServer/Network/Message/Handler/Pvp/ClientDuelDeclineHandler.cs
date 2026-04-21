using NexusForever.Game.Pvp;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Pvp;

namespace NexusForever.WorldServer.Network.Message.Handler.Pvp
{
    public class ClientDuelDeclineHandler : IMessageHandler<IWorldSession, ClientDuelDecline>
    {
        public void HandleMessage(IWorldSession session, ClientDuelDecline _)
        {
            DuelManager.Instance.HandleDecline(session.Player);
        }
    }
}
