using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.Network.World.Message.Static;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.SpellForceRemoveChanneled)]
    public class SpellEffectSpellForceRemoveChanneledHandler : ISpellEffectApplyHandler<ISpellEffectSpellForceRemoveData>
    {
        private readonly ILogger<SpellEffectSpellForceRemoveChanneledHandler> log;

        public SpellEffectSpellForceRemoveChanneledHandler(
            ILogger<SpellEffectSpellForceRemoveChanneledHandler> log)
        {
            this.log = log;
        }

        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectSpellForceRemoveData data)
        {
            foreach (ISpell spellToRemove in GetMatchingSpells(target, data).Where(IsChanneled).ToList())
                spellToRemove.CancelCast(CastResult.SpellCancelled);

            return SpellEffectExecutionResult.Ok;
        }

        private IEnumerable<ISpell> GetMatchingSpells(IUnitEntity target, ISpellEffectSpellForceRemoveData data)
        {
            switch (data.Type)
            {
                case SpellEffectForceSpellRemoveType.SpellGroupId:
                    return target.GetSpellsByGroupId(data.Data);
                case SpellEffectForceSpellRemoveType.Spell4:
                    return ToEnumerable(target.GetSpellBySpellId(data.Data));
                case SpellEffectForceSpellRemoveType.SpellBase:
                    return ToEnumerable(target.GetSpellByBaseSpellId(data.Data));
                default:
                    log.LogWarning("Unhandled EffectForceSpellRemoveType {Type} for channeled remove.", data.Type);
                    return [];
            }
        }

        private static bool IsChanneled(ISpell spell)
        {
            return spell is { IsFinished: false }
                && spell.CastMethod is CastMethod.Channeled or CastMethod.ChanneledField;
        }

        private static IEnumerable<ISpell> ToEnumerable(ISpell spell)
        {
            if (spell != null)
                yield return spell;
        }
    }
}
