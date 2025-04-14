using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectModifyInterruptArmourData : ISpellEffectModifyInterruptArmourData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public uint InterruptArmour { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry           = entry;
            InterruptArmour = entry.DataBits00;
        }
    }
}
