using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectForcedMoveData : ISpellEffectForcedMoveData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public SpellEffectForcedMoveType MoveType { get; private set; }
        public float MinDistance { get; private set; }
        public float MaxDistance { get; private set; }
        public TimeSpan FlightTime { get; private set; }
        public float Gravity { get; private set; }
        public SpellEffectForcedMoveFlags Flags { get; private set; }
        public float Unknown6 { get; private set; }
        public float Angle { get; private set; }
        public float Spin { get; private set; }
        public uint Unknown9 { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry       = entry;
            MoveType    = (SpellEffectForcedMoveType)entry.DataBits00;
            MinDistance = BitConverter.UInt32BitsToSingle(entry.DataBits01);
            MaxDistance = BitConverter.UInt32BitsToSingle(entry.DataBits02);
            FlightTime  = TimeSpan.FromMilliseconds(entry.DataBits03);
            Gravity     = BitConverter.UInt32BitsToSingle(entry.DataBits04);
            Flags       = (SpellEffectForcedMoveFlags)entry.DataBits05;
            Unknown6    = BitConverter.UInt32BitsToSingle(entry.DataBits06);
            Angle       = BitConverter.UInt32BitsToSingle(entry.DataBits07);
            Spin        = BitConverter.UInt32BitsToSingle(entry.DataBits08);
            Unknown9    = entry.DataBits09;
        }
    }
}
