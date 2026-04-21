using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Abstract.Spell.Effect.Data
{
    public interface ISpellEffectModifySpellCooldownData : ISpellEffectData
    {
        SpellEffectModifySpellCooldownType Type { get; }
        uint Data { get; }
        double Cooldown { get; }
    }
}
