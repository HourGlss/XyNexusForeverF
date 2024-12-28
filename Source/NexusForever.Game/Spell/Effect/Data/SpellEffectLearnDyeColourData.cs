using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectLearnDyeColourData : ISpellEffectLearnDyeColourData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public uint GenericUnlockEntryId { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry                = entry;
            GenericUnlockEntryId = entry.DataBits00;
        }
    }
}
