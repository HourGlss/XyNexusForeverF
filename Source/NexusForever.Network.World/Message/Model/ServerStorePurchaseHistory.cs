using NexusForever.Game.Static.Account;
using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    [Message(GameMessageOpcode.ServerStorePurchaseHistory)]
    public class ServerStorePurchaseHistory : IWritable
    {
        public class Purchase : IWritable
        {
            public ulong PurchaseId { get; set; }
            public ulong TransactionDateTime { get; set; }
            public AccountCurrencyType CurrencyId { get; set; }
            public byte Unknown0 { get; set; }
            public float Cost { get; set; }
            public string Name { get; set; }
            public bool Unknown1 { get; set; }
            public ulong Unknown2 { get; set; }
            public uint Unknown3 { get; set; }

            public void Write(GamePacketWriter writer)
            {
                writer.Write(PurchaseId);
                writer.Write(TransactionDateTime);
                writer.Write(CurrencyId, 5u);
                writer.Write(Unknown0, 3u);
                writer.Write(Cost);
                writer.WriteStringWide(Name);
                writer.Write(Unknown1);
                writer.Write(Unknown2);
                writer.Write(Unknown3);
            }
        }

        public List<Purchase> Purchases { get; set; } = [];

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Purchases.Count);
            Purchases.ForEach(p => p.Write(writer));
        }
    }
}
