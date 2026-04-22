namespace NexusForever.Game.Abstract.Spell.Effect.Data
{
    public interface ISpellEffectSummonPetData : ISpellEffectData
    {
        uint CreatureId { get; }
        uint AutoAttackDelayMs { get; }
    }
}
