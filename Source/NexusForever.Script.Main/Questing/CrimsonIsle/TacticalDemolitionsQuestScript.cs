using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Questing.CrimsonIsle
{
    [ScriptFilterOwnerId(5604)]
    public class TacticalDemolitionsQuestScript : IQuestScript, IOwnedScript<IQuest>
    {
        private const uint ExileCannonsObjectiveId = 8268u;
        private const uint CinematicCompleteObjectiveId = 15918u;

        private IQuest owner;

        public void OnLoad(IQuest owner)
        {
            this.owner = owner;
        }

        public void OnObjectiveUpdate(IQuestObjective objective)
        {
            if (objective.ObjectiveInfo.Id != ExileCannonsObjectiveId || !objective.IsComplete())
                return;

            if (owner.State != QuestState.Achieved)
                owner.ObjectiveUpdate(CinematicCompleteObjectiveId, 1u);
        }
    }
}
