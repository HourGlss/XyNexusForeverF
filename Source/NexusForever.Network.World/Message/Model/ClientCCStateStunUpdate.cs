using NexusForever.Game.Static.Combat.CrowdControl;
using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    [Message(GameMessageOpcode.ClientCCStateStunUpdate)]
    public class ClientCCStateStunUpdate : IReadable
    {
        public CCStateStunVictimGameplay Pressed { get; private set; }
        public CCStateStunVictimGameplay Held { get; private set; }

        public void Read(GamePacketReader reader)
        {
            Pressed = reader.ReadEnum<CCStateStunVictimGameplay>();
            Held    = reader.ReadEnum<CCStateStunVictimGameplay>();
        }
    }
}
