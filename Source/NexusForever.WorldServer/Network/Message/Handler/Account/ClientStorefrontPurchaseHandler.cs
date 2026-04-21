using NexusForever.Game.Abstract.Storefront;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler.Account
{
    public class ClientStorefrontPurchaseHandler : IMessageHandler<IWorldSession, ClientStorefrontPurchase>
    {
        #region Dependency Injection

        private readonly IGlobalStorefrontManager globalStorefrontManager;

        public ClientStorefrontPurchaseHandler(
            IGlobalStorefrontManager globalStorefrontManager)
        {
            this.globalStorefrontManager = globalStorefrontManager;
        }

        #endregion

        public void HandleMessage(IWorldSession session, ClientStorefrontPurchase storefrontPurchase)
        {
            IOfferItem offerItem = globalStorefrontManager.GetStoreOfferItem(storefrontPurchase.Purchase.OfferId);
            session.PurchaseManager.PurchaseOffer(offerItem, storefrontPurchase.Purchase.CurrencyType, storefrontPurchase.Purchase.Cost);
        }
    }
}
