namespace NexusForever.Game.Abstract.Spell.Effect.Data
{
    public interface ISpellEffectProxyData : ISpellEffectData
    {
        uint SpellId { get; }
        uint PeriodicSpellId { get;}
        uint MaxExecutions { get; }
        uint PrerequisiteId { get; }
    }
}
