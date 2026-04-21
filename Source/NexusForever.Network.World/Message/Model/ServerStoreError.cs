using NexusForever.Game.Static.Storefront;
using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    [Message(GameMessageOpcode.ServerStoreError)]
    public class ServerStoreError : IWritable
    {
        public StoreError ErrorCode { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(ErrorCode, 5u);
        }
    }
}
