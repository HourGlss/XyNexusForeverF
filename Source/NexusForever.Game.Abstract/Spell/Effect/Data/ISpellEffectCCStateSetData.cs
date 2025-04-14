using NexusForever.GameTable.Model;

namespace NexusForever.Game.Abstract.Spell.Effect.Data
{
    public interface ISpellEffectCCStateSetData : ISpellEffectData
    {
        CCStatesEntry CCState { get; }
        uint InterruptArmourReduction { get; }
        CCStateAdditionalDataEntry CCStateAdditionalDataEntry { get; }
    }
}
