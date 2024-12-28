using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectProxyData : ISpellEffectProxyData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public uint SpellId { get; private set; }
        public uint PeriodicSpellId { get; private set; }
        public uint MaxExecutions { get; private set; }
        public uint PrerequisiteId { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry           = entry;
            SpellId         = entry.DataBits00;
            PeriodicSpellId = entry.DataBits01;
            MaxExecutions   = entry.DataBits04;
            PrerequisiteId  = entry.DataBits06;
        }
    }
}
