using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    [Message(GameMessageOpcode.ServerSpellEffectExecute)]
    public class ServerSpellEffectExecute : IWritable
    {
        public uint CastingId { get; set; }
        public uint TargetGuid { get; set; }
        public uint SpellEffectUniqueId { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(CastingId);
            writer.Write(TargetGuid);
            writer.Write(SpellEffectUniqueId);
        }
    }
}
