using NexusForever.Database;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Quest;
using NexusForever.Shared;

namespace NexusForever.Game.Abstract.Quest
{
    public interface IQuest : IDisposable, IUpdate, IDatabaseCharacter, IDatabaseState, IEnumerable<IQuestObjective>
    {
        ushort Id { get; }
        IQuestInfo Info { get; }
        QuestState State { get; set; }
        QuestStateFlags Flags { get; set; }
        uint? Timer { get; set; }
        DateTime? Reset { get; set; }

        /// <summary>
        /// Create a new <see cref="IQuest"/> from an existing database model.
        /// </summary>
        void Initialise(IPlayer owner, IQuestInfo info, CharacterQuestModel model);

        /// <summary>
        /// Create a new <see cref="IQuest"/> from supplied <see cref="IQuestInfo"/>.
        /// </summary>
        void Initialise(IPlayer owner, IQuestInfo info);

        /// <summary>
        /// Returns if <see cref="IQuest"/> can be deleted.
        /// </summary>
        bool CanDelete();

        /// <summary>
        /// Returns if <see cref="IQuest"/> can be abandoned.
        /// </summary>
        bool CanAbandon();

        /// <summary>
        /// Returns if <see cref="IQuest"/> can be shared with another <see cref="IPlayer"/>.
        /// </summary>
        bool CanShare();

        /// <summary>
        /// Returns the owner <see cref="IPlayer"/> of the <see cref="IQuest"/>.
        /// </summary>
        IPlayer GetOwner();

        /// <summary>
        /// Return the <see cref="IQuestObjective"/> with the supplied id.
        /// </summary>
        IQuestObjective GetQuestObjective(uint id);

        /// <summary>
        /// Return the <see cref="IQuestObjective"/> with the supplied index.
        /// </summary>
        IQuestObjective GetQuestObjectiveByIndex(byte index);

        /// <summary>
        /// Complete the <see cref="IQuest"/> without rewards.
        /// </summary>
        void CompleteQuest();

        /// <summary>
        /// Complete the <see cref="IQuest"/> with the specified reward.
        /// </summary>
        void RewardQuest(ushort reward);

        /// <summary>
        /// Update any <see cref="IQuestObjective"/>'s with supplied <see cref="QuestObjectiveType"/> and data with progress.
        /// </summary>
        void ObjectiveUpdate(QuestObjectiveType type, uint data, uint progress);

        /// <summary>
        /// Update any <see cref="IQuestObjective"/>'s with supplied ID with progress.
        /// </summary>
        void ObjectiveUpdate(uint id, uint progress);

        /// <summary>
        /// Notify the quest that an item was added to the owner's inventory.
        /// </summary>
        void NotifyItemAdded(uint itemId);

        /// <summary>
        /// Notify the quest that the owner gained a title.
        /// </summary>
        void NotifyTitleAdded(ushort titleId);
    }
}
