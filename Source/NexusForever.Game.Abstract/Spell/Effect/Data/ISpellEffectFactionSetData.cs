using NexusForever.Game.Static.Reputation;

namespace NexusForever.Game.Abstract.Spell.Effect.Data
{
    public interface ISpellEffectFactionSetData : ISpellEffectData
    {
        Faction FactionId { get; }
    }
}
