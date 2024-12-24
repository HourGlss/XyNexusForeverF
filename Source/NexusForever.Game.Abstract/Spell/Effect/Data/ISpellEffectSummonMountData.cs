namespace NexusForever.Game.Abstract.Spell.Effect.Data
{
    public interface ISpellEffectSummonMountData : ISpellEffectData
    {
        uint CreatureId { get; }
        uint VehicleId { get; }
        uint ItemDisplayId { get; }
    }
}
