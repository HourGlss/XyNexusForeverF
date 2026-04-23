using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Questing.NorthernWilds
{
    [ScriptFilterCreatureId(11205)]
    public class LoftiteCrystalEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const ushort QuestId = 3486;
        private const uint CollectCrystalObjectiveId = 4485u;

        private IWorldEntity owner;

        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
            owner.SetInRangeCheck(5f);
        }

        public void OnEnterRange(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            if (player.QuestManager.GetQuestState(QuestId) != QuestState.Accepted)
                return;

            player.QuestManager.ObjectiveUpdate(CollectCrystalObjectiveId, 1u);
            owner.RemoveFromMap();
        }
    }
}
