using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Shared;

namespace NexusForever.Network.World.Message.Model.AccountInventory
{
    [Message(GameMessageOpcode.ServerAccountItemInProgressUpdate)]
    public class ServerAccountItemInProgressUpdate : IWritable
    {
        public uint AccountId { get; set; }
        public AccountInventoryItem Item { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(AccountId);
            Item.Write(writer);
        }
    }
}
