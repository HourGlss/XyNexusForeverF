using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Reputation;
using NexusForever.Script.Template;

namespace NexusForever.Script.Main.Tutorial.Script.Part1
{
    public abstract class NavigatingNexusSequentialEntityScript : ICanSeeMeScript, IOwnedScript<IWorldEntity>
    {
        protected IWorldEntity owner;

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public virtual void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
        }

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

            IQuest quest = player.QuestManager.GetActiveQuest(questId);
            if (quest == null)
                return false;

            return owner.QuestChecklistIdx switch
            {
                0 => !quest.GetQuestObjective(21271).IsComplete(),
                1 => quest.GetQuestObjective(21271).IsComplete()
                    && !quest.GetQuestObjective(21279).IsComplete(),
                2 => quest.GetQuestObjective(21279).IsComplete()
                    && !quest.GetQuestObjective(21272).IsComplete(),
                3 => quest.GetQuestObjective(21272).IsComplete(),
                _ => false,
            };
        }
    }
}
