using NexusForever.Game.Abstract.Entity;

namespace NexusForever.Game.Abstract.Spell.Proc
{
    public interface IProcParameters
    {
        public IUnitEntity Target { get; set; }
    }
}
