using NexusForever.Game.Static.Storefront;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler.Account
{
    public class ClientStorefrontPurchaseGiftHandler : IMessageHandler<IWorldSession, ClientStorefrontPurchaseGift>
    {
        public void HandleMessage(IWorldSession session, ClientStorefrontPurchaseGift _)
        {
            // Gifting requires cross-character account inventory delivery, which this codebase does not expose yet.
            session.EnqueueMessageEncrypted(new ServerStoreError
            {
                ErrorCode = StoreError.GenericFail
            });
        }
    }
}
