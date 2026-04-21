using System;
using System.Collections.Generic;
using System.Linq;
using NexusForever.Database;
using NexusForever.Database.Auth;
using NexusForever.Database.Auth.Model;
using NexusForever.Game.Abstract.Storefront;
using NexusForever.Game.Prerequisite;
using NexusForever.Game.Static.Account;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Storefront;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model;
using NexusForever.Shared.Game.Events;
using NexusForever.WorldServer.Network;
using NLog;

namespace NexusForever.WorldServer.Storefront
{
    public class PurchaseManager : IPurchaseManager
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private readonly IWorldSession session;
        private readonly List<StoreTransaction> transactions = [];

        public bool TransactionsAllowed { get; private set; }

        public PurchaseManager(IWorldSession session, AccountModel model)
        {
            this.session = session;

            foreach (AccountStoreTransactionModel transactionModel in model.AccountStoreTransaction)
                transactions.Add(new StoreTransaction(transactionModel));

            TransactionsAllowed = true;
        }

        public void PurchaseOffer(IOfferItem offerItem, AccountCurrencyType currencyType, float? clientCost = null)
        {
            IOfferItemPrice priceData = offerItem?.GetPriceDataForCurrency(currencyType);
            float cost = priceData?.GetCurrencyValue() ?? 0f;

            StoreError storeError = CheckPurchase(offerItem, currencyType, cost, clientCost);
            if (storeError != StoreError.Success)
            {
                session.EnqueueMessageEncrypted(new ServerStoreError
                {
                    ErrorCode = storeError
                });
                return;
            }

            TransactionsAllowed = false;
            CreateTransaction(offerItem, currencyType, cost, completedTransaction =>
            {
                transactions.Add(new StoreTransaction(completedTransaction));
                GrantOfferItems(offerItem);
                session.Account.CurrencyManager.CurrencySubtractAmount(currencyType, (ulong)cost);

                session.EnqueueMessageEncrypted(new ServerStorePurchaseResult
                {
                    Success   = true,
                    ErrorCode = StoreError.Success
                });

                TransactionsAllowed = true;
            });
        }

        private StoreError CheckPurchase(IOfferItem offerItem, AccountCurrencyType currencyType, float cost, float? clientCost)
        {
            if (!TransactionsAllowed)
                return StoreError.PurchasePending;

            if (offerItem == null)
                return StoreError.InvalidOffer;

            if (offerItem.GetPriceDataForCurrency(currencyType) == null)
                return StoreError.InvalidPrice;

            if (clientCost.HasValue && Math.Abs(clientCost.Value - cost) > 0.01f)
                return StoreError.InvalidPrice;

            if (!session.Account.CurrencyManager.CanAfford(currencyType, (ulong)cost))
                return StoreError.InvalidPrice;

            if (session.Player != null && offerItem.Items.Any(item => item.Entry.PrerequisiteId != 0u
                    && !PrerequisiteManager.Instance.Meets(session.Player, item.Entry.PrerequisiteId)))
                return StoreError.CannotUseOffer;

            return StoreError.Success;
        }

        private void CreateTransaction(IOfferItem offerItem, AccountCurrencyType currencyType, float cost, Action<AccountStoreTransactionModel> callback)
        {
            var transaction = new AccountStoreTransactionModel
            {
                Id                  = session.Account.Id,
                Name                = offerItem.Name,
                CurrencyType        = (ushort)currencyType,
                Cost                = cost,
                TransactionDateTime = DateTime.UtcNow
            };

            session.Events.EnqueueEvent(new TaskGenericEvent<AccountStoreTransactionModel>(
                DatabaseManager.Instance.GetDatabase<AuthDatabase>().CreateStoreTransactionAsync(transaction),
                callback));
        }

        private void GrantOfferItems(IOfferItem offerItem)
        {
            foreach (IOfferItemData itemData in offerItem.Items)
                GrantOfferItem(itemData);
        }

        private void GrantOfferItem(IOfferItemData itemData)
        {
            AccountItemEntry entry = itemData.Entry;

            if (entry.AccountCurrencyEnum != 0u && entry.AccountCurrencyAmount != 0ul)
                session.Account.CurrencyManager.CurrencyAddAmount((AccountCurrencyType)entry.AccountCurrencyEnum, entry.AccountCurrencyAmount * itemData.Amount);

            if (entry.EntitlementIdPurchase != 0u)
                session.Account.EntitlementManager.UpdateEntitlement((EntitlementType)entry.EntitlementIdPurchase, (int)Math.Max(1u, itemData.Amount));

            if (entry.GenericUnlockSetId == 0u)
                return;

            GenericUnlockEntryEntry unlockEntry = GameTableManager.Instance.GenericUnlockEntry.GetEntry(entry.GenericUnlockSetId);
            if (unlockEntry == null)
            {
                log.Warn($"Store purchase {entry.Id} references invalid generic unlock {entry.GenericUnlockSetId}.");
                return;
            }

            session.Account.GenericUnlockManager.Unlock((ushort)unlockEntry.Id);
        }

        public void SendPurchaseHistory()
        {
            session.EnqueueMessageEncrypted(new ServerStorePurchaseHistory
            {
                Purchases = transactions
                    .Select(t => t.Build())
                    .ToList()
            });
        }
    }
}
