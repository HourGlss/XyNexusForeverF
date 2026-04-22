using NexusForever.Game.Static.Setting;
using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    // Sent after ServerInstanceSettings; later option changes use the smaller option packets.
    [Message(GameMessageOpcode.ClientCombatOptions)]
    public class ClientCombatOptions : IReadable
    {
        public CastingOptionFlags CastingOptions { get; private set; }
        public bool DisableOtherPlayersLogging { get; private set; }
        public CombatLogOptions CombatLogDisableFlags { get; private set; }

        public void Read(GamePacketReader reader)
        {
            CastingOptions = reader.ReadEnum<CastingOptionFlags>(4u);
            DisableOtherPlayersLogging = reader.ReadBit();
            CombatLogDisableFlags = reader.ReadEnum<CombatLogOptions>(14u);
        }
    }
}
