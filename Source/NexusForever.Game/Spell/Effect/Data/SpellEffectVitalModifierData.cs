using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Static.Entity;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectVitalModifierData : ISpellEffectVitalModifierData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public Vital Vital { get; private set; }
        public uint Value { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry = entry;
            Vital = (Vital)entry.DataBits00;
            Value = entry.DataBits01;
        }
    }
}
