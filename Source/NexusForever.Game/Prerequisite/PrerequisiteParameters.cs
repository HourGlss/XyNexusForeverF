using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.Network.World.Message.Static;

namespace NexusForever.Game.Prerequisite
{
    public class PrerequisiteParameters : IPrerequisiteParameters
    {
        public CastResult? CastResult { get; set; }
        public ISpellInfo SpellInfo { get; set; }
        public IUnitEntity Target { get; set; }
        public ushort? TaxiNode { get; set; }
    }
}
