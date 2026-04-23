using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Questing.CrimsonIsle
{
    [ScriptFilterCreatureId(31792)]
    public class MegatechWarbotEntityScript : INonPlayerScript, IOwnedScript<INonPlayerEntity>
    {
        private const ushort QuestId = 5594;
        private const uint WarbotObjectiveId = 8249u;
        private const ushort WarbotAchievementId = 1730;

        public void OnLoad(INonPlayerEntity owner)
        {
        }

        public void OnDeath(IUnitEntity killer)
        {
            if (killer is not IPlayer player)
                return;

            if (player.QuestManager.GetQuestState(QuestId) != QuestState.Accepted)
                return;

            player.AchievementManager.GrantAchievement(WarbotAchievementId);
            player.QuestManager.ObjectiveUpdate(WarbotObjectiveId, 1u);
        }
    }
}
