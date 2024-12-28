using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Abstract.Spell.Effect.Data
{
    public interface ISpellEffectSpellForceRemoveData : ISpellEffectData
    {
        SpellEffectForceSpellRemoveType Type { get; }
        uint Data { get; }
    }
}
