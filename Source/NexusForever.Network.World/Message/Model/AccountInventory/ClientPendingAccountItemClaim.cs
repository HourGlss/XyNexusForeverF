using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model.AccountInventory
{
    [Message(GameMessageOpcode.ClientPendingAccountItemClaim)]
    public class ClientPendingAccountItemClaim : IReadable
    {
        public string TransactionId { get; private set; }

        public void Read(GamePacketReader reader)
        {
            TransactionId = reader.ReadWideString();
        }
    }
}
