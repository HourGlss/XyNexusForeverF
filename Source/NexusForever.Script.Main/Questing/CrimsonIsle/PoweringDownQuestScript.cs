using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Questing.CrimsonIsle
{
    [ScriptFilterOwnerId(5573)]
    public class PoweringDownQuestScript : IQuestScript, IOwnedScript<IQuest>
    {
        private const uint PowerRegulatorsObjectiveId = 8229u;
        private const uint CinematicCompleteObjectiveId = 12870u;

        private IQuest owner;

        public void OnLoad(IQuest owner)
        {
            this.owner = owner;
        }

        public void OnObjectiveUpdate(IQuestObjective objective)
        {
            if (objective.ObjectiveInfo.Id != PowerRegulatorsObjectiveId || !objective.IsComplete())
                return;

            if (owner.State != QuestState.Achieved)
                owner.ObjectiveUpdate(CinematicCompleteObjectiveId, 1u);
        }
    }
}
