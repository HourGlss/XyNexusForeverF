using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectSummonCreatureData : ISpellEffectSummonCreatureData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public uint CreatureId { get; private set; }
        public uint Count { get; private set; }
        public float MinDistance { get; private set; }
        public float MaxDistance { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry       = entry;
            CreatureId  = entry.DataBits00;
            Count       = entry.DataBits02 == 0u ? 1u : entry.DataBits02;
            MinDistance = DecodeDistance(entry.DataBits03);
            MaxDistance = DecodeDistance(entry.DataBits04);
        }

        private static float DecodeDistance(uint value)
        {
            if (value == 0u)
                return 0f;

            float decoded = BitConverter.UInt32BitsToSingle(value);
            if (float.IsFinite(decoded) && decoded >= 0.001f && decoded <= 1000f)
                return decoded;

            return value;
        }
    }
}
