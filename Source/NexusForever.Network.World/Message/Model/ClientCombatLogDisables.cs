using NexusForever.Game.Static.Setting;
using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    [Message(GameMessageOpcode.ClientCombatLogDisables)]
    public class ClientCombatLogDisables : IReadable
    {
        public CombatLogOptions DisableFlags { get; private set; }

        public void Read(GamePacketReader reader)
        {
            DisableFlags = reader.ReadEnum<CombatLogOptions>(32u);
        }
    }
}
