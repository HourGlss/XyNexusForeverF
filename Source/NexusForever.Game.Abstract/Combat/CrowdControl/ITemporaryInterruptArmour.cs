using NexusForever.Game.Abstract.Spell.Target;

namespace NexusForever.Game.Abstract.Combat.CrowdControl
{
    public interface ITemporaryInterruptArmour
    {
        ISpellTargetEffectInfo Info { get; }
        uint Amount { get; set; }
    }
}
