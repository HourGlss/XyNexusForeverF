using NexusForever.Game.Static.Entity;

namespace NexusForever.Game.Abstract.Spell.Effect.Data
{
    public interface ISpellEffectUnitPropertyModifierData : ISpellEffectData
    {
        Property Property { get; }
        uint Priority { get; }
        float PercentageModifier { get; }
        float FlatValueModifier { get; }
        float LevelScalingModifier { get; }
    }
}
