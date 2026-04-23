namespace NexusForever.Game.Abstract.Spell.Effect.Data
{
    public interface ISpellEffectSummonCreatureData : ISpellEffectData
    {
        uint CreatureId { get; }
        uint Count { get; }
        float MinDistance { get; }
        float MaxDistance { get; }
    }
}
