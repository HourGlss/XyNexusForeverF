using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Shared;

namespace NexusForever.Network.World.Message.Model.Story
{
    // If neither IsEmote nor IsTextBubble is set, the client renders NPC say chat plus a bubble.
    [Message(GameMessageOpcode.ServerStoryTextUnit)]
    public class ServerStoryTextUnit : IWritable
    {
        public StoryMessage StoryMessage { get; set; }
        public uint UnitId { get; set; }
        public bool IsEmote { get; set; }
        public bool IsTextBubble { get; set; }
        public float TextBubbleRange { get; set; }

        public void Write(GamePacketWriter writer)
        {
            StoryMessage.Write(writer);
            writer.Write(UnitId);
            writer.Write(IsEmote);
            writer.Write(IsTextBubble);
            writer.Write(TextBubbleRange);
        }
    }
}
