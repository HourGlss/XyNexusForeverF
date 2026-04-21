using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectModifySpellCooldownData : ISpellEffectModifySpellCooldownData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public SpellEffectModifySpellCooldownType Type { get; private set; }
        public uint Data { get; private set; }
        public double Cooldown { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry = entry;
            Type = (SpellEffectModifySpellCooldownType)entry.DataBits00;
            Data = entry.DataBits01;
            Cooldown = BitConverter.Int32BitsToSingle((int)entry.DataBits02);
        }
    }
}
