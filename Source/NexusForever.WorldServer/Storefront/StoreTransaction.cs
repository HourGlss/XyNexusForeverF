using System;
using NexusForever.Database.Auth.Model;
using NexusForever.Game.Static.Account;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Storefront
{
    public class StoreTransaction
    {
        public ulong PurchaseId { get; }
        public string Name { get; }
        public AccountCurrencyType CurrencyType { get; }
        public float Cost { get; }
        public DateTime PurchaseDateTime { get; }

        public StoreTransaction(AccountStoreTransactionModel model)
        {
            PurchaseId       = model.TransactionId;
            Name             = model.Name;
            CurrencyType     = (AccountCurrencyType)model.CurrencyType;
            Cost             = model.Cost;
            PurchaseDateTime = model.TransactionDateTime;
        }

        public ServerStorePurchaseHistory.Purchase Build()
        {
            return new ServerStorePurchaseHistory.Purchase
            {
                PurchaseId          = PurchaseId,
                Name                = Name,
                CurrencyId          = CurrencyType,
                Cost                = Cost,
                TransactionDateTime = (ulong)PurchaseDateTime.ToFileTimeUtc()
            };
        }
    }
}
