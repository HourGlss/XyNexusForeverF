namespace NexusForever.Game.Static.Storefront
{
    public enum StoreError
    {
        CatalogUnavailable,
        StoreDisabled,
        InvalidOffer,
        InvalidPrice,
        GenericFail,
        PurchasePending,
        PgWsCartFraudFailure,
        PgWsCartPaymentFailure,
        PgWsInvalidCCExpirationDate,
        PgWsInvalidCreditCardNumber,
        PgWsCreditCardExpired,
        PgWsCreditCardDeclined,
        PgWsCreditFloorExceeded,
        PgWsInventoryStatusFailure,
        PgWsPaymentPostAuthFailure,
        PgWsSubmitCartFailed,
        PurchaseVelocityLimit,
        MissingItemEntitlement,
        IneligibleGiftRecipient,
        CannotUseOffer,
        MissingEntitlement,
        CannotGiftOffer,
        Success = 22
    }
}
