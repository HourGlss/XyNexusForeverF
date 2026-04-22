using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model.AccountInventory
{
    [Message(GameMessageOpcode.ServerAccountItemInProgressRemove)]
    public class ServerAccountItemInProgressRemove : IWritable
    {
        public uint AccountId { get; set; }
        public ulong UserInventoryId { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(AccountId);
            writer.Write(UserInventoryId);
        }
    }
}
