using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectUnlockPetFlairData : ISpellEffectUnlockPetFlairData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public uint PetFlairId { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry      = entry;
            PetFlairId = entry.DataBits00;
        }
    }
}
