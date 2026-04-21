using NexusForever.Game.Static.Storefront;
using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    [Message(GameMessageOpcode.ServerStorePurchaseResult)]
    public class ServerStorePurchaseResult : IWritable
    {
        public bool Success { get; set; }
        public StoreError ErrorCode { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Success);
            writer.Write(ErrorCode, 5u);
        }
    }
}
