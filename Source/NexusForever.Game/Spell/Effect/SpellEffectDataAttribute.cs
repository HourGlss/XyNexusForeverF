using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell.Effect
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SpellEffectDataAttribute : Attribute
    {
        public SpellEffectType SpellEffectType { get; }

        public SpellEffectDataAttribute(SpellEffectType spellEffectType)
        {
            SpellEffectType = spellEffectType;
        }
    }
}
