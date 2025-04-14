using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    [Message(GameMessageOpcode.ServerSpellUpdateEffectDuration)]
    public class ServerSpellUpdateEffectDuration : IWritable
    {
        public uint CastingId { get; set; }
        public uint SpellEffectUniqueId { get; set; }
        public int Duration { get; set; }
        public uint TargetId { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(CastingId);
            writer.Write(SpellEffectUniqueId);
            writer.Write(Duration);
            writer.Write(TargetId);
        }
    }
}
