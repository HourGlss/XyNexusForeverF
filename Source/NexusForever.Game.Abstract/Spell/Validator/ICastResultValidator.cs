using NexusForever.Network.World.Message.Static;

namespace NexusForever.Game.Abstract.Spell.Validator
{
    public interface ICastResultValidator
    {
        CastResult GetCastResult(ISpell spell);
    }
}
