using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Static.Combat.CrowdControl;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectCCStateBreakData : ISpellEffectCCStateBreakData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public CCState CCState { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry   = entry;
            CCState = (CCState)entry.DataBits00;
        }
    }
}
