using NexusForever.Game.Static.Combat.CrowdControl;
using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    [Message(GameMessageOpcode.ServerEntityCCStateRemove)]
    public class ServerEntityCCStateRemove : IWritable
    {
        public uint Guid { get; set; }
        public CCState CCState { get; set; }
        public uint CastingId { get; set; }
        public uint EffectUniqueId { get; set; }
        public bool Removed { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Guid);
            writer.Write(CCState, 5);
            writer.Write(CastingId);
            writer.Write(EffectUniqueId);
            writer.Write(Removed);
        }
    }
}
