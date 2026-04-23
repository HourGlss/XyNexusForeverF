using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.GameTable;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Main.Creature.City.Thayd
{
    [ScriptFilterCreatureId(70385)]
    public class TeleportToDefileEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint HopesDareWorldLocationId = 49900u;

        public void OnLoad(IWorldEntity owner)
        {
        }

        public void OnActivateSuccess(IPlayer activator)
        {
            if (!activator.CanTeleport())
                return;

            var entry = GameTableManager.Instance.WorldLocation2.GetEntry(HopesDareWorldLocationId);
            if (entry == null)
                return;

            var rotation = new Quaternion(entry.Facing0, entry.Facing1, entry.Facing2, entry.Facing3);
            activator.Rotation = rotation.ToEulerDegrees();
            activator.TeleportTo((ushort)entry.WorldId, entry.Position0, entry.Position1, entry.Position2);
        }
    }
}
