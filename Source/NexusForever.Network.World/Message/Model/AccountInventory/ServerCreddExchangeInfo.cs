using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model.AccountInventory
{
    [Message(GameMessageOpcode.ServerCreddExchangeInfo)]
    public class ServerCreddExchangeInfo : IWritable
    {
        public class CreddListing : IWritable
        {
            public ulong ListingId { get; set; }
            public ulong Price { get; set; }
            public bool IsBuyOrder { get; set; }
            public ulong ListTime { get; set; }
            public ulong ExpirationTime { get; set; }

            public void Write(GamePacketWriter writer)
            {
                writer.Write(ListingId);
                writer.Write(Price);
                writer.Write(IsBuyOrder);
                writer.Write(ListTime);
                writer.Write(ExpirationTime);
            }
        }

        public uint BuyOrderCount { get; set; }
        public ulong[] BuyOrderPrices { get; set; } = new ulong[3];
        public uint SellOrderCount { get; set; }
        public ulong[] SellOrderPrices { get; set; } = new ulong[3];
        public List<CreddListing> Listings { get; set; } = [];

        public void Write(GamePacketWriter writer)
        {
            writer.Write(BuyOrderCount);
            foreach (ulong price in BuyOrderPrices)
                writer.Write(price);

            writer.Write(SellOrderCount);
            foreach (ulong price in SellOrderPrices)
                writer.Write(price);

            writer.Write(Listings.Count);
            Listings.ForEach(listing => listing.Write(writer));
        }
    }
}
