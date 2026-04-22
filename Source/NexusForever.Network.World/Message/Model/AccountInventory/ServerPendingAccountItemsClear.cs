using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model.AccountInventory
{
    [Message(GameMessageOpcode.ServerPendingAccountItemsClear)]
    public class ServerPendingAccountItemsClear : IWritable
    {
        public void Write(GamePacketWriter writer)
        {
        }
    }
}
