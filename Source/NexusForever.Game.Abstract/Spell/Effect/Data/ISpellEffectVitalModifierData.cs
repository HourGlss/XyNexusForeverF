using NexusForever.Game.Static.Entity;

namespace NexusForever.Game.Abstract.Spell.Effect.Data
{
    public interface ISpellEffectVitalModifierData : ISpellEffectData
    {
        Vital Vital { get; }
        int Value { get; }
    }
}
