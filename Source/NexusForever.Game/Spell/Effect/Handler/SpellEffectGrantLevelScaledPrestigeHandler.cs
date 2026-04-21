using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.GrantLevelScaledPrestige)]
    public class SpellEffectGrantLevelScaledPrestigeHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        private readonly IGameTableManager gameTableManager;

        public SpellEffectGrantLevelScaledPrestigeHandler(
            IGameTableManager gameTableManager)
        {
            this.gameTableManager = gameTableManager;
        }

        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (target is not IPlayer player)
                return SpellEffectExecutionResult.PreventEffect;

            ulong amount = GetLevelScaledAmount(player, data);
            if (amount == 0u)
                return SpellEffectExecutionResult.PreventEffect;

            player.CurrencyManager.CurrencyAddAmount(CurrencyType.Prestige, amount, true);
            return SpellEffectExecutionResult.Ok;
        }

        private uint GetLevelScaledAmount(IPlayer player, ISpellEffectDefaultData data)
        {
            if (data.DataBits00 != 0u)
                return data.DataBits00;

            XpPerLevelEntry entry = gameTableManager.XpPerLevel.GetEntry(player.Level);
            return entry?.BaseQuestXpPerLevel ?? 0u;
        }
    }
}
