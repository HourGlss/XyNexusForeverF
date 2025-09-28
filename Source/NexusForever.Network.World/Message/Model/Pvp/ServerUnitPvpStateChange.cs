using NexusForever.Game.Static.PVP;
using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model.Pvp
{
    [Message(GameMessageOpcode.ServerUnitPvpStateChange)]
    public class ServerUnitPvpStateChange : IWritable
    {
        public uint UnitId { get; set; }
        public PvPFlag State { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(UnitId);
            writer.Write(State, 3u);
        }
    }
}
