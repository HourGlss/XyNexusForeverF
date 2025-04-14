using NexusForever.Game.Abstract.Combat.CrowdControl;
using NexusForever.Game.Abstract.Spell.Target;

namespace NexusForever.Game.Combat.CrowdControl
{
    public class TemporaryInterruptArmour : ITemporaryInterruptArmour
    {
        public ISpellTargetEffectInfo Info { get; }
        public uint Amount { get; set; }

        public TemporaryInterruptArmour(ISpellTargetEffectInfo info, uint amount)
        {
            Info   = info;
            Amount = amount;
        }
    }
}
