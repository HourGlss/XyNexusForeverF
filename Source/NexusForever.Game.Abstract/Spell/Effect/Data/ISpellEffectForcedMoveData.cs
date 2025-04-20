using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Abstract.Spell.Effect.Data
{
    public interface ISpellEffectForcedMoveData : ISpellEffectData
    {
        SpellEffectForcedMoveType MoveType { get; }
        float MinDistance { get; }
        float MaxDistance { get; }
        TimeSpan FlightTime { get; }
        float Gravity { get; }
        SpellEffectForcedMoveFlags Flags { get; }
        float Unknown6 { get; }
        float Angle { get; }
        float Spin { get; }
        uint Unknown9 { get; }
    }
}
