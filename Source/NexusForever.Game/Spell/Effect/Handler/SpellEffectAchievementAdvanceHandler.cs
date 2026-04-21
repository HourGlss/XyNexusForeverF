using NexusForever.Game.Abstract.Achievement;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Achievement;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.AchievementAdvance)]
    public class SpellEffectAchievementAdvanceHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        private readonly IGlobalAchievementManager globalAchievementManager;

        public SpellEffectAchievementAdvanceHandler(
            IGlobalAchievementManager globalAchievementManager)
        {
            this.globalAchievementManager = globalAchievementManager;
        }

        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (target is not IPlayer player)
                return SpellEffectExecutionResult.PreventEffect;

            if (data.DataBits00 == 0u)
                return SpellEffectExecutionResult.PreventEffect;

            if (data.DataBits01 == 0u && data.DataBits02 == 0u && data.DataBits00 <= ushort.MaxValue)
            {
                ushort achievementId = (ushort)data.DataBits00;
                IAchievementInfo achievement = globalAchievementManager.GetAchievement(achievementId);
                if (achievement == null)
                    return SpellEffectExecutionResult.PreventEffect;

                if (!player.AchievementManager.HasCompletedAchievement(achievementId))
                    player.AchievementManager.GrantAchievement(achievementId);
            }
            else
                player.AchievementManager.CheckAchievements(player, (AchievementType)data.DataBits00, data.DataBits01, data.DataBits02, data.DataBits03 == 0u ? 1u : data.DataBits03);

            return SpellEffectExecutionResult.Ok;
        }
    }
}
