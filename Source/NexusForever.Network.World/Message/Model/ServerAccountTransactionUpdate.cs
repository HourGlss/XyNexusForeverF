using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    [Message(GameMessageOpcode.ServerAccountTransactionUpdate)]
    public class ServerAccountTransactionUpdate : IWritable
    {
        public string TransactionGuid { get; set; }
        public bool Redeemed { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.WriteStringWide(TransactionGuid);
            writer.Write(Redeemed);
        }
    }
}
