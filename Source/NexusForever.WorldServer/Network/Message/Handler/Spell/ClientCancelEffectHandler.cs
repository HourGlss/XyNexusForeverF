using System.Linq;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Spell;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler.Spell
{
    public class ClientCancelEffectHandler : IMessageHandler<IWorldSession, ClientCancelEffect>
    {
        public void HandleMessage(IWorldSession session, ClientCancelEffect cancelSpell)
        {
            ISpell spell = session.Player.GetSpell(cancelSpell.ServerUniqueId);
            if (spell == null)
                return;

            // Stalker stealth toggles already have explicit on/off spell paths. Some clients send an
            // immediate cancel-effect request against the active stealth spell, which tears the status
            // down right after it is applied. Let RemoveStealth/toggle-off handle the exit instead.
            if (spell.GetTarget(session.Player)?.GetEffectsByType(SpellEffectType.Stealth).Any() == true)
                return;

            spell.Finish();
        }
    }
}
