using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model.AccountInventory
{
    [Message(GameMessageOpcode.ClientCreddRedeem)]
    public class ClientCreddRedeem : IReadable
    {
        public void Read(GamePacketReader reader)
        {
        }
    }
}
