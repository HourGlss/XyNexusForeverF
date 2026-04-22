using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model.AccountInventory
{
    [Message(GameMessageOpcode.ClientCreddExchangeBuyOrderSubmit)]
    public class ClientCreddExchangeBuyOrderSubmit : IReadable
    {
        public ulong Bid { get; private set; }
        public bool Immediate { get; private set; }

        public void Read(GamePacketReader reader)
        {
            Bid = reader.ReadULong();
            Immediate = reader.ReadBit();
        }
    }
}
