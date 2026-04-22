using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Shared;

namespace NexusForever.Network.World.Message.Model.AccountInventory
{
    [Message(GameMessageOpcode.ClientAccountItemGiftToAccountFriend)]
    public class ClientAccountItemGiftToAccountFriend : IReadable
    {
        public string TransactionId { get; private set; }
        public ulong RecipientAccountFriendId { get; private set; }
        public uint Unused { get; private set; }
        public Identity Gifter { get; private set; } = new();

        public void Read(GamePacketReader reader)
        {
            TransactionId = reader.ReadWideString();
            RecipientAccountFriendId = reader.ReadULong();
            Unused = reader.ReadUInt();
            Gifter.Read(reader);
        }
    }
}
