using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.ModifySpellCooldown)]
    public class SpellEffectModifySpellCooldownHandler : ISpellEffectApplyHandler<ISpellEffectModifySpellCooldownData>
    {
        #region Dependency Injection

        private readonly ILogger<SpellEffectModifySpellCooldownHandler> log;

        public SpellEffectModifySpellCooldownHandler(
            ILogger<SpellEffectModifySpellCooldownHandler> log)
        {
            this.log = log;
        }

        #endregion

        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectModifySpellCooldownData data)
        {
            if (target is not IPlayer player)
                return SpellEffectExecutionResult.Ok;

            switch (data.Type)
            {
                case SpellEffectModifySpellCooldownType.Spell4:
                    player.SpellManager.SetSpellCooldown(data.Data, data.Cooldown, true);
                    break;
                case SpellEffectModifySpellCooldownType.SpellCooldownId:
                    player.SpellManager.SetSpellCooldownByCooldownId(data.Data, data.Cooldown);
                    break;
                default:
                    log.LogWarning($"Unhandled ModifySpellCooldown Type {data.Type}");
                    break;
            }

            return SpellEffectExecutionResult.Ok;
        }
    }
}
