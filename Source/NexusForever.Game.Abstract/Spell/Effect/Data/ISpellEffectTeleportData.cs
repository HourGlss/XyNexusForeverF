namespace NexusForever.Game.Abstract.Spell.Effect.Data
{
    public interface ISpellEffectTeleportData : ISpellEffectData
    {
        public uint WorldLocationId { get; }
    }
}
