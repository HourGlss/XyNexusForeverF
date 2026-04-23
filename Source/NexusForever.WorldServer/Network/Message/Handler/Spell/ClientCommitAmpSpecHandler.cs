using System.Linq;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Abilities;

namespace NexusForever.WorldServer.Network.Message.Handler.Spell
{
    public class ClientCommitAmpSpecHandler : IMessageHandler<IWorldSession, ClientCommitAmpSpec>
    {
        public void HandleMessage(IWorldSession session, ClientCommitAmpSpec commitAmpSpec)
        {
            IActionSet actionSet = session.Player.SpellManager.GetActionSet(session.Player.SpellManager.ActiveActionSet);
            if (actionSet.SyncAmps(commitAmpSpec.Amps))
            {
                session.Player.SpellManager.GrantSpells();
                session.EnqueueMessageEncrypted(actionSet.BuildServerAmpList());
            }
        }
    }
}
