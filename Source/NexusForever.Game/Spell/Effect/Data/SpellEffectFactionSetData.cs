using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Static.Reputation;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectFactionSetData : ISpellEffectFactionSetData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public Faction FactionId { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry     = entry;
            FactionId = (Faction)entry.DataBits00;
        }
    }
}
