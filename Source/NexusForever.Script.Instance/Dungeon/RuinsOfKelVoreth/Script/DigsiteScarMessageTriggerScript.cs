using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Trigger;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Map.Instance;

namespace NexusForever.Script.Instance.Dungeon.RuinsOfKelVoreth.Script
{
    [ScriptFilterOwnerId(847)]
    public class DigsiteScarMessageTriggerScript : IGridEntityScript, IOwnedScript<IGridTriggerEntity>
    {
        protected IGridTriggerEntity trigger;

        #region Dependency Injection

        private readonly IGlobalQuestManager globalQuestManager;

        public DigsiteScarMessageTriggerScript(

            IGlobalQuestManager globalQuestManager)
        {
            this.globalQuestManager = globalQuestManager;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IGridTriggerEntity owner)
        {
            trigger = owner;
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to range check range.
        /// </summary>
        public void OnEnterRange(IGridEntity entity)
        {
            if (entity.Map is not IMapInstance mapInstance)
                return;

            ICommunicatorMessage message = globalQuestManager.GetCommunicatorMessage(CommunicatorMessage.AvraDarkos5);
            foreach (IPlayer player in mapInstance.GetPlayers())
                message?.Send(player.Session);
            //TODO: Send the correct faction message for every player
        }
    }
}
