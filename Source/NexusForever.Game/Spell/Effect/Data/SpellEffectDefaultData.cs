using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectDefaultData : ISpellEffectData, ISpellEffectDefaultData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public uint DataBits00 { get; private set; }
        public uint DataBits01 { get; private set; }
        public uint DataBits02 { get; private set; }
        public uint DataBits03 { get; private set; }
        public uint DataBits04 { get; private set; }
        public uint DataBits05 { get; private set; }
        public uint DataBits06 { get; private set; }
        public uint DataBits07 { get; private set; }
        public uint DataBits08 { get; private set; }
        public uint DataBits09 { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry      = entry;
            DataBits00 = entry.DataBits00;
            DataBits01 = entry.DataBits01;
            DataBits02 = entry.DataBits02;
            DataBits03 = entry.DataBits03;
            DataBits04 = entry.DataBits04;
            DataBits05 = entry.DataBits05;
            DataBits06 = entry.DataBits06;
            DataBits07 = entry.DataBits07;
            DataBits08 = entry.DataBits08;
            DataBits09 = entry.DataBits09;
        }
    }
}
