namespace NexusForever.Game.Spell.Info
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SpellInfoPatchSpellBaseIdAttribute : Attribute
    {
        public uint BaseSpellId { get; }

        public SpellInfoPatchSpellBaseIdAttribute(uint baseSpellId)
        {
            BaseSpellId = baseSpellId;
        }
    }
}
