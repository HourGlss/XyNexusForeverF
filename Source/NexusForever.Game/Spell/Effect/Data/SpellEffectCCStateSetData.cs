using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    [SpellEffectData(SpellEffectType.CCStateSet)]
    public class SpellEffectCCStateSetData : ISpellEffectCCStateSetData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public CCStatesEntry CCState { get; private set; }
        public uint InterruptArmourReduction { get; private set; }
        public CCStateAdditionalDataEntry CCStateAdditionalDataEntry { get; private set; }

        #region Dependency Injection
            
        private readonly IGameTableManager gameTableManager;

        public SpellEffectCCStateSetData(
            IGameTableManager gameTableManager)
        {
            this.gameTableManager = gameTableManager;
        }

        #endregion

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry                      = entry;
            CCState                    = gameTableManager.CCStates.GetEntry(entry.DataBits00);
            InterruptArmourReduction   = entry.DataBits03;

            // TODO: this probably needs to be handled similar to the spell effect data
            CCStateAdditionalDataEntry = gameTableManager.CCStateAdditionalData.GetEntry(entry.DataBits07);
        }
    }
}
