using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Shared;

namespace NexusForever.Network.World.Message.Model
{
    [Message(GameMessageOpcode.ClientStorefrontPurchase)]
    public class ClientStorefrontPurchase : IReadable
    {
        public StorefrontPurchase Purchase { get; } = new();

        public void Read(GamePacketReader reader)
        {
            Purchase.Read(reader);
        }
    }
}
