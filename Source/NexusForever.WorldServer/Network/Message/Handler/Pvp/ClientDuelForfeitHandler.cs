using NexusForever.Game.Pvp;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Pvp;

namespace NexusForever.WorldServer.Network.Message.Handler.Pvp
{
    public class ClientDuelForfeitHandler : IMessageHandler<IWorldSession, ClientDuelForfeit>
    {
        public void HandleMessage(IWorldSession session, ClientDuelForfeit _)
        {
            DuelManager.Instance.HandleForfeit(session.Player);
        }
    }
}
