using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectSummonVanityPetData : ISpellEffectSummonVanityPetData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public uint CreatureId { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry      = entry;
            CreatureId = entry.DataBits00;
        }
    }
}
