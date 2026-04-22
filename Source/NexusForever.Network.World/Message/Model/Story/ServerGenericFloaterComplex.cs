using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Shared;

namespace NexusForever.Network.World.Message.Model.Story
{
    // Appears over the player's unit; commonly used for localized text such as "Evade".
    [Message(GameMessageOpcode.ServerGenericFloaterComplex)]
    public class ServerGenericFloaterComplex : IWritable
    {
        public uint LocalizedTextId { get; set; }
        public uint RandomTextLineId { get; set; }
        public List<StoryMessage> Messages { get; set; } = [];

        public void Write(GamePacketWriter writer)
        {
            writer.Write(LocalizedTextId);
            writer.Write(RandomTextLineId);
            writer.Write(Messages.Count, 8u);
            Messages.ForEach(message => message.Write(writer));
        }
    }
}
