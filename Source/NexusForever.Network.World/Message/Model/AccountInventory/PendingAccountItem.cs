using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Shared;

namespace NexusForever.Network.World.Message.Model.AccountInventory
{
    public class PendingAccountItem : IWritable
    {
        public ulong PendingItemGroupId { get; set; }
        public uint AccountItemId { get; set; }
        public ulong Unused2 { get; set; }
        public string TransactionId { get; set; }
        public uint Unused3 { get; set; }
        public Identity Unused4 { get; set; } = new();
        public ulong Unused5 { get; set; }
        public byte Unknown { get; set; }
        public ulong Unused6 { get; set; }
        public Identity Unused7 { get; set; } = new();

        public void Write(GamePacketWriter writer)
        {
            writer.Write(PendingItemGroupId);
            writer.Write(AccountItemId);
            writer.Write(Unused2);
            writer.WriteStringWide(TransactionId);
            writer.Write(Unused3);
            Unused4.Write(writer);
            writer.Write(Unused5);
            writer.Write(Unknown, 5u);
            writer.Write(Unused6);
            Unused7.Write(writer);
        }
    }
}
