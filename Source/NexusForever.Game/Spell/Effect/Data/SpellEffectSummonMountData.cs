using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectSummonMountData : ISpellEffectSummonMountData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public uint CreatureId { get; private set; }
        public uint VehicleId { get; private set; }
        public uint ItemDisplayId { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry         = entry;
            CreatureId    = entry.DataBits00;
            VehicleId     = entry.DataBits01;
            ItemDisplayId = entry.DataBits04;
        }
    }
}
