using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Reputation;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Tutorial.Script.Part1
{
    [ScriptFilterCreatureId(73610)]
    public class BarrierEntityScript : ICanSeeMeScript, IOwnedScript<IWorldEntity>
    {
        /// <summary>
        /// Determines whether the specified <see cref="IGridEntity"/> can see this entity.
        /// </summary>
        public bool CanSeeMe(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return true;

            QuestId questId = player.Faction2 == Faction.Exile
                ? QuestId.NavigatingNexusPt2Exile
                : QuestId.NavigatingNexusPt2Dominion;

            IQuest quest = player.QuestManager.GetActiveQuest(questId);
            if (quest == null)
                return true;

            return !quest.GetQuestObjective(21324).IsComplete();
        }
    }
}
