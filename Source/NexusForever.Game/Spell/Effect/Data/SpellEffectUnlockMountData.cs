using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectUnlockMountData : ISpellEffectUnlockMountData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public uint SpellId { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry   = entry;
            SpellId = entry.DataBits00;
        }
    }
}
