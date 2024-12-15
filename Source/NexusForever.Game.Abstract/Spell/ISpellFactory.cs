using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Abstract.Spell
{
    public interface ISpellFactory
    {
        ISpell CreateSpell(CastMethod method);
    }
}
