using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Reputation;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Tutorial.Script.Part1
{
    [ScriptFilterCreatureId(73735)]
    public class CombatHologramEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> is successfully activated by <see cref="IPlayer"/>.
        /// </summary>
        public void OnActivateSuccess(IPlayer activator)
        {
            if (activator.Faction2 != Faction.Exile)
                return;

            activator.TeleportToLocal(new Vector3(-172.937f, -879.5415f, 263.561f));
        }
    }
}
