using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell.Proc;

namespace NexusForever.Game.Spell.Proc
{
    public class ProcParameters : IProcParameters
    {
        public IUnitEntity Target { get; set; }
    }
}
