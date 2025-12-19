using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Quest;
using NexusForever.Game.Static.Reputation;
using NexusForever.Script.Template;

namespace NexusForever.Script.Main.Tutorial.Script.Part1
{
    public abstract class NavigatingNexusSequentialPt2EntityScript : ICanSeeMeScript
    {
        /// <summary>
        /// Determines whether the specified <see cref="IGridEntity"/> can see this entity.
        /// </summary>
        public bool CanSeeMe(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return false;

            QuestId questId = player.Faction2 == Faction.Exile
                ? QuestId.NavigatingNexusExile
                : QuestId.NavigatingNexusDominion;

            if (player.QuestManager.GetQuestState(questId) != QuestState.Completed)
                return false;

            QuestId questPt2Id = player.Faction2 == Faction.Exile
                ? QuestId.NavigatingNexusPt2Exile
                : QuestId.NavigatingNexusPt2Dominion;

            IQuest quest = player.QuestManager.GetActiveQuest(questPt2Id);
            if (quest == null)
                return false;

            return !quest.GetQuestObjective(21321).IsComplete();
        }
    }
}
