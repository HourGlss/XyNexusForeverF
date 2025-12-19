using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Tutorial.Script.Part1
{
    [ScriptFilterCreatureId(73595)]
    public class FinishLineEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IWorldEntity owner)
        {
            owner.SetInRangeCheck(10f);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to range check range.
        /// </summary>
        public void OnEnterRange(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            player.QuestManager.ObjectiveUpdate(QuestObjectiveType.EnterArea, 8560, 1);
        }
    }
}
