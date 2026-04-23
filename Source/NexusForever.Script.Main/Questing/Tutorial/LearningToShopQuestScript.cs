using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Questing.Tutorial
{
    [ScriptFilterOwnerId(10510)]
    public class LearningToShopQuestScript : IQuestScript, IOwnedScript<IQuest>
    {
        private const uint ObjectiveId = 21267u;
        private const uint SmartShopperItemId = 86245u;
        private const ushort SmartShopperTitleId = 400;

        private IQuest owner;

        public void OnLoad(IQuest owner)
        {
            this.owner = owner;
        }

        public void OnQuestStateChange(QuestState newState, QuestState oldState)
        {
            if (newState == QuestState.Accepted)
                TryCompleteObjective();
        }

        public void OnItemAdded(uint itemId)
        {
            if (itemId == SmartShopperItemId)
                TryCompleteObjective();
        }

        public void OnTitleAdded(ushort titleId)
        {
            if (titleId == SmartShopperTitleId)
                TryCompleteObjective();
        }

        private void TryCompleteObjective()
        {
            if (owner.State != QuestState.Accepted)
                return;

            IPlayer player = owner.GetOwner();
            if (!player.Inventory.HasItem(SmartShopperItemId) && !player.TitleManager.HasTitle(SmartShopperTitleId))
                return;

            owner.ObjectiveUpdate(ObjectiveId, 1u);
        }
    }
}
