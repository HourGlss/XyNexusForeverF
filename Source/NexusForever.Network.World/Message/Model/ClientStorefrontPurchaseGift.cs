using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Shared;

namespace NexusForever.Network.World.Message.Model
{
    [Message(GameMessageOpcode.ClientStorefrontPurchaseGift)]
    public class ClientStorefrontPurchaseGift : IReadable
    {
        public StorefrontPurchase Purchase { get; } = new();
        public uint Unknown1 { get; private set; }
        public Identity Target { get; } = new();
        public string Unknown2 { get; private set; }

        public void Read(GamePacketReader reader)
        {
            Purchase.Read(reader);
            Unknown1 = reader.ReadUInt();
            Target.Read(reader);
            Unknown2 = reader.ReadWideString();
        }
    }
}
