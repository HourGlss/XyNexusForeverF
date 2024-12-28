using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    [SpellEffectData(SpellEffectType.Damage)]
    public class SpellEffectDamageData : ISpellEffectDamageData
    {
        public Spell4EffectsEntry Entry { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry = entry;
        }
    }
}
