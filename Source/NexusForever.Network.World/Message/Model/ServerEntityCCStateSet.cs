using NexusForever.Game.Static.Combat.CrowdControl;
using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    [Message(GameMessageOpcode.ServerEntityCCStateSet)]
    public class ServerEntityCCStateSet : IWritable
    {
        public uint Guid { get; set; }
        public CCState CCState { get; set; }
        public uint EffectUniqueId { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Guid);
            writer.Write(CCState, 5);
            writer.Write(EffectUniqueId);
        }
    }
}
