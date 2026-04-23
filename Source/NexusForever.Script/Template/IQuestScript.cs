using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Quest;
using NexusForever.Shared;

namespace NexusForever.Script.Template
{
    public interface IQuestScript : IUpdate
    {
        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        void IUpdate.Update(double lastTick)
        {
        }

        /// <summary>
        /// Invoked when <see cref="QuestState"/> is update for <see cref="IQuest"/>.
        /// </summary>
        void OnQuestStateChange(QuestState newState, QuestState oldState)
        {
        }

        /// <summary>
        /// Invoked when <see cref="IQuestObjective"/> is updated for <see cref="IQuest"/>.
        /// </summary>
        void OnObjectiveUpdate(IQuestObjective objective)
        {
        }

        /// <summary>
        /// Invoked when an item is added to the owning player's inventory while this quest is active.
        /// </summary>
        void OnItemAdded(uint itemId)
        {
        }

        /// <summary>
        /// Invoked when the owning player gains a title while this quest is active.
        /// </summary>
        void OnTitleAdded(ushort titleId)
        {
        }
    }
}
