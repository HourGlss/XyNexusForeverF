using NexusForever.GameTable.Model;

namespace NexusForever.Game.Abstract.Spell.Effect.Data
{
    public interface ISpellEffectData
    {
        Spell4EffectsEntry Entry { get; }

        void Populate(Spell4EffectsEntry entry);
    }
}
