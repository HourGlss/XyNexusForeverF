using NexusForever.Game.Static.AccountInventory;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Shared;

namespace NexusForever.Network.World.Message.Model.AccountInventory
{
    [Message(GameMessageOpcode.ServerCreddOperationHistory)]
    public class ServerCreddOperationHistory : IWritable
    {
        public class CreddOperationHistoryEntry : IWritable
        {
            public AccountOperation Operation { get; set; }
            public bool IsInitiator { get; set; }
            public uint LogAge { get; set; }
            public Identity Transactor { get; set; } = new();
            public Identity Unused { get; set; } = new();
            public ulong AccountFriendId { get; set; }
            public ulong MoneyAmount { get; set; }

            public void Write(GamePacketWriter writer)
            {
                writer.Write(Operation);
                writer.Write(IsInitiator);
                writer.Write(LogAge);
                Transactor.Write(writer);
                Unused.Write(writer);
                writer.Write(AccountFriendId);
                writer.Write(MoneyAmount);
            }
        }

        public List<CreddOperationHistoryEntry> HistoryEntries { get; set; } = [];

        public void Write(GamePacketWriter writer)
        {
            writer.Write(HistoryEntries.Count);
            HistoryEntries.ForEach(entry => entry.Write(writer));
        }
    }
}
