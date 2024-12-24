using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectSpellForceRemoveData : ISpellEffectSpellForceRemoveData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public SpellEffectForceSpellRemoveType Type { get; private set; }
        public uint Data { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry = entry;
            Type  = (SpellEffectForceSpellRemoveType)entry.DataBits00;
            Data  = entry.DataBits01;
        }
    }
}
