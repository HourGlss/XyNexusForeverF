using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Abilities;

namespace NexusForever.WorldServer.Network.Message.Handler.Spell
{
    public class ClientSetStanceHandler : IMessageHandler<IWorldSession, ClientSetStance>
    {
        public void HandleMessage(IWorldSession session, ClientSetStance innateChange)
        {
            session.Player.SpellManager.SetInnate(innateChange.InnateIndex);
        }
    }
}
