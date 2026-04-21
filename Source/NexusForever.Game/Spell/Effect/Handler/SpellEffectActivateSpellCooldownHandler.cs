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
    [SpellEffectHandler(SpellEffectType.ActivateSpellCooldown)]
    public class SpellEffectActivateSpellCooldownHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        private readonly ILogger<SpellEffectActivateSpellCooldownHandler> log;
        private readonly ISpellInfoManager spellInfoManager;

        public SpellEffectActivateSpellCooldownHandler(
            ILogger<SpellEffectActivateSpellCooldownHandler> log,
            ISpellInfoManager spellInfoManager)
        {
            this.log              = log;
            this.spellInfoManager = spellInfoManager;
        }

        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (target is not IPlayer player)
                return SpellEffectExecutionResult.PreventEffect;

            if (!SpellEffectCooldownHelper.TryReadSelector(data, executionContext.Spell, out SpellEffectModifySpellCooldownType type, out uint value))
                return SpellEffectExecutionResult.PreventEffect;

            double fallbackCooldown = executionContext.Spell.Parameters.SpellInfo.Entry.SpellCoolDown / 1000d;
            double cooldown         = SpellEffectCooldownHelper.ReadCooldownSeconds(data, fallbackCooldown);
            if (cooldown <= 0d)
                return SpellEffectExecutionResult.PreventEffect;

            return SpellEffectCooldownHelper.TryApplyCooldown(player, spellInfoManager, type, value, cooldown, log)
                ? SpellEffectExecutionResult.Ok
                : SpellEffectExecutionResult.PreventEffect;
        }
    }
}
