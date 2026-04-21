using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    [Message(GameMessageOpcode.ClientFallingDamage)]
    public class ClientFallingDamage : IReadable
    {
        public float HealthPercent { get; private set; }

        public void Read(GamePacketReader reader)
        {
            HealthPercent = reader.ReadSingle();
        }
    }
}
