using NexusForever.Game.Abstract.Storefront;
using NexusForever.Game.Static.Account;

namespace NexusForever.WorldServer.Storefront
{
    public interface IPurchaseManager
    {
        bool TransactionsAllowed { get; }

        void PurchaseOffer(IOfferItem offerItem, AccountCurrencyType currencyType, float? clientCost = null);
        void SendPurchaseHistory();
    }
}
