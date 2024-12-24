using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Event;

namespace NexusForever.Game.Abstract.Spell
{
    public interface IProxy
    {
        bool CanCast { get; }
        ISpellEffectProxyData Data { get; }
        ISpell ParentSpell { get; }
        IUnitEntity Target { get; }

        void Cast(IUnitEntity caster, ISpellEventManager events);
        void Evaluate();
    }
}