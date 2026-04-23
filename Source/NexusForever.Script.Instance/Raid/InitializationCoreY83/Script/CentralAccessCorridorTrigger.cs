using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Trigger;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Map.Instance;

namespace NexusForever.Script.Instance.Raid.InitializationCoreY83.Script
{
    [ScriptFilterOwnerId(2681)]
    public class CentralAccessCorridorTriggerScript : IGridEntityScript, IOwnedScript<IGridTriggerEntity>
    {
        protected IGridTriggerEntity trigger;

        #region Dependency Injection

        private readonly IGlobalQuestManager globalQuestManager;

        public CentralAccessCorridorTriggerScript(

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

            //doesn't plays till the Caretaker is finished speaking
            ICommunicatorMessage message = globalQuestManager.GetCommunicatorMessage(CommunicatorMessage.Nurton1);
            foreach (IPlayer player in mapInstance.GetPlayers())
                message?.Send(player.Session);
            //This may need to trigger for every player, but for now I will leave it as a group trigger
            //Note: this may not even be a trigger at all, but i am 90% sure it's a trigger
        }
    }
}
