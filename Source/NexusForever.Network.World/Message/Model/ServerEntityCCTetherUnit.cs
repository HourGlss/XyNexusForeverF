using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    [Message(GameMessageOpcode.ServerEntityCCTetherUnit)]
    public class ServerEntityCCTetherUnit : IWritable
    {
        public uint Guid { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Guid);
        }
    }
}
