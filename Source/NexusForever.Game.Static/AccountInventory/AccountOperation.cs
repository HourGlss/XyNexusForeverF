namespace NexusForever.Game.Static.AccountInventory
{
    public enum AccountOperation
    {
        MtxPurchase              = 0x00,
        ClaimPending             = 0x01,
        ReturnPending            = 0x02,
        TakeItem                 = 0x03,
        GiftItem                 = 0x04,
        RedeemCoupon             = 0x05,
        GetCreddExchangeInfo     = 0x06,
        SellCredd                = 0x07,
        BuyCredd                 = 0x08,
        CancelCreddOrder         = 0x09,
        ExpireCreddOrder         = 0x0A,
        SellCreddComplete        = 0x0B,
        BuyCreddComplete         = 0x0C,
        CreddRedeem              = 0x0F,
        RequestDailyLoginRewards = 0x10,
        RequestPremiumLockboxKey = 0x11,
    }
}
