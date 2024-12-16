using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.SpellForceRemove)]
    public class SpellEffectSpellForceRemoveHandler : ISpellEffectApplyHandler
    {
        #region Dependency Injection

        private readonly ILogger<SpellEffectSpellForceRemoveHandler> log;

        public SpellEffectSpellForceRemoveHandler(
            ILogger<SpellEffectSpellForceRemoveHandler> log)
        {
            this.log = log;
        }

        #endregion

        public void Apply(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info)
        {
            switch ((SpellEffectForceSpellRemoveType)info.Entry.DataBits00)
            {
                case SpellEffectForceSpellRemoveType.SpellGroupId:
                {
                    foreach (ISpell spellToRemove in target.GetSpellsByGroupId(info.Entry.DataBits01))
                        spellToRemove.Finish();
                    break;
                }
                case SpellEffectForceSpellRemoveType.Spell4:
                {
                    ISpell spellToRemove = target.GetSpellBySpellId(info.Entry.DataBits01);
                    spellToRemove?.Finish();
                    break;
                }
                case SpellEffectForceSpellRemoveType.SpellBase:
                {
                    ISpell spellToRemove = target.GetSpellByBaseSpellId(info.Entry.DataBits01);
                    spellToRemove?.Finish();
                    break;
                }
                default:
                    log.LogWarning($"Unhandled EffectForceSpellRemoveType Type {(SpellEffectForceSpellRemoveType)info.Entry.DataBits00}");
                    break;
            }
        }
    }
}
