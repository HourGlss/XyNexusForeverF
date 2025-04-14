using NexusForever.Game.Static.Combat.CrowdControl;
using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    [Message(GameMessageOpcode.ServerCCStateStunDirection)]
    public class ServerCCStateStunDirection : IWritable
    {
        public CCStateStunVictimGameplay Direction { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Direction, 8);
        }
    }
}
