using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Shared;

namespace NexusForever.Network.World.Message.Model.AccountInventory
{
    [Message(GameMessageOpcode.ClientCreddExchangeSellOrderSubmit)]
    public class ClientCreddExchangeSellOrderSubmit : IReadable
    {
        public Identity Unused { get; private set; } = new();
        public ulong Price { get; private set; }
        public bool Immediate { get; private set; }

        public void Read(GamePacketReader reader)
        {
            Unused.Read(reader);
            Price = reader.ReadULong();
            Immediate = reader.ReadBit();
        }
    }
}
