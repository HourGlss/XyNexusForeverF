using NexusForever.Network.World.Message.Static;

namespace NexusForever.Game.Abstract.Spell.Validator
{
    public interface ICastResultValidatorManager
    {
        CastResult GetCastResult(ISpell spell);
    }
}
