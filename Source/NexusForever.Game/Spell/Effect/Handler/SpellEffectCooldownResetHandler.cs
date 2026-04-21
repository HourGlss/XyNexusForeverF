using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.CooldownReset)]
    public class SpellEffectCooldownResetHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        private readonly ILogger<SpellEffectCooldownResetHandler> log;
        private readonly ISpellInfoManager spellInfoManager;

        public SpellEffectCooldownResetHandler(
            ILogger<SpellEffectCooldownResetHandler> log,
            ISpellInfoManager spellInfoManager)
        {
            this.log              = log;
            this.spellInfoManager = spellInfoManager;
        }

        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (target is not IPlayer player)
                return SpellEffectExecutionResult.PreventEffect;

            if (data.DataBits00 == 0u && data.DataBits01 == 0u)
            {
                player.SpellManager.ResetAllSpellCooldowns();
                return SpellEffectExecutionResult.Ok;
            }

            if (!SpellEffectCooldownHelper.TryReadSelector(data, executionContext.Spell, out SpellEffectModifySpellCooldownType type, out uint value))
                return SpellEffectExecutionResult.PreventEffect;

            return SpellEffectCooldownHelper.TryApplyCooldown(player, spellInfoManager, type, value, 0d, log)
                ? SpellEffectExecutionResult.Ok
                : SpellEffectExecutionResult.PreventEffect;
        }
    }
}
