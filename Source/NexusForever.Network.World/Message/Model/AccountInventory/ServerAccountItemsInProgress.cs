using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Shared;

namespace NexusForever.Network.World.Message.Model.AccountInventory
{
    [Message(GameMessageOpcode.ServerAccountItemsInProgress)]
    public class ServerAccountItemsInProgress : IWritable
    {
        public uint AccountId { get; set; }
        public List<AccountInventoryItem> Items { get; set; } = [];

        public void Write(GamePacketWriter writer)
        {
            writer.Write(AccountId);
            writer.Write(Items.Count);
            Items.ForEach(item => item.Write(writer));
        }
    }
}
