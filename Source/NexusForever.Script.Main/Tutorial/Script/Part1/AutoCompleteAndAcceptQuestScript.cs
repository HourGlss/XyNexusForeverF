using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;

namespace NexusForever.Script.Main.Tutorial.Script.Part1
{
    public abstract class AutoCompleteAndAcceptQuestScript : IQuestScript, IOwnedScript<IQuest>
    {
        public abstract QuestId NextQuestId { get; }

        protected IQuest owner;

        /// <summary>
        /// Invoked when <see cref="IQuestScript"/> is loaded.
        /// </summary>
        public void OnLoad(IQuest owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Invoked when <see cref="QuestState"/> is update for <see cref="IQuest"/>.
        /// </summary>
        public void OnQuestStateChange(QuestState newState, QuestState oldState)
        {
            if (newState != QuestState.Achieved)
                return;

            IPlayer player = owner.GetOwner();
            player.QuestManager.QuestComplete(owner.Id);
            player.QuestManager.QuestAdd(NextQuestId);
        }
    }
}
