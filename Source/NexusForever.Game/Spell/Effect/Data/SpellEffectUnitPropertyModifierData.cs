using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Static.Entity;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectUnitPropertyModifierData : ISpellEffectUnitPropertyModifierData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public Property Property { get; private set; }
        public uint Priority { get; private set; }
        public float PercentageModifier { get; private set; }
        public float FlatValueModifier { get; private set; }
        public float LevelScalingModifier { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry                = entry;
            Property             = (Property)entry.DataBits00;
            Priority             = entry.DataBits01;
            PercentageModifier   = BitConverter.UInt32BitsToSingle(entry.DataBits02);
            FlatValueModifier    = BitConverter.UInt32BitsToSingle(entry.DataBits03);
            LevelScalingModifier = BitConverter.UInt32BitsToSingle(entry.DataBits04);
        }
    }
}
