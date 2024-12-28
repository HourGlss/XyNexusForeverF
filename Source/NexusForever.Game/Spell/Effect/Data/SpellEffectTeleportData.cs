using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectTeleportData : ISpellEffectTeleportData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public uint WorldLocationId { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry           = entry;
            WorldLocationId = entry.DataBits00;
        }
    }
}
