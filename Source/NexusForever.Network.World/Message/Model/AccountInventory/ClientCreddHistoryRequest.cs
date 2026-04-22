using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model.AccountInventory
{
    [Message(GameMessageOpcode.ClientCreddHistoryRequest)]
    public class ClientCreddHistoryRequest : IReadable
    {
        public void Read(GamePacketReader reader)
        {
        }
    }
}
