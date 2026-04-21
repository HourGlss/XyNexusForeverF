using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler.Account
{
    public class ClientStorefrontRequestPurchaseHistoryHandler : IMessageHandler<IWorldSession, ClientStorefrontRequestPurchaseHistory>
    {
        public void HandleMessage(IWorldSession session, ClientStorefrontRequestPurchaseHistory _)
        {
            session.PurchaseManager.SendPurchaseHistory();
        }
    }
}
