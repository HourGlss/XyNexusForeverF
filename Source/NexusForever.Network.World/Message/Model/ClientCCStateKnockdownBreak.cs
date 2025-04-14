using NexusForever.Game.Static.Entity;
using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    [Message(GameMessageOpcode.ClientCCStateKnockdownBreak)]
    public class ClientCCStateKnockdownBreak : IReadable
    {
        public DashDirection Direction { get; private set; }

        public void Read(GamePacketReader reader)
        {
            Direction = reader.ReadEnum<DashDirection>(3u);
        }
    }
}
