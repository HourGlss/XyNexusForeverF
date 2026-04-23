using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Adventure.WarOfTheWilds
{
    [ScriptFilterOwnerId(158)]
    public class WarOfTheWildsAdventureEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;
        private IMapInstance mapInstance;


        #region Dependency Injection

        private readonly IGlobalQuestManager globalQuestManager;

        public WarOfTheWildsAdventureEventScript(
            IGlobalQuestManager globalQuestManager)
        {
            this.globalQuestManager = globalQuestManager;
        }

        #endregion

        public void OnLoad(IPublicEvent owner)
        {
            publicEvent = owner;
            mapInstance = publicEvent.Map as IMapInstance;

            publicEvent.SetPhase(PublicEventPhase.Fight);
        }

        public void OnAddToMap(IGridEntity entity)
        {
            switch (entity)
            {
                case IWorldEntity worldEntity:
                    OnAddToMapWorldEntity(worldEntity);
                    break;
            }
        }

        private void OnAddToMapWorldEntity(IWorldEntity worldEntity)
        {
            switch ((PublicEventCreature)worldEntity.CreatureId)
            {

            }
        }
        public void OnPublicEventPhase(uint phase)
        {
            switch ((PublicEventPhase)phase)
            {
                case PublicEventPhase.Fight:
                    OnPhaseFight();
                    break;
            }
        }

        private void OnPhaseFight()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DestroyTheGiantMoodieTotem);
            publicEvent.ActivateObjective(PublicEventObjective.MoodieTotemHealth);
            publicEvent.ActivateObjective(PublicEventObjective.SkeechTotemHealth);
        }
        public void OnPublicEventObjectiveStatus(IPublicEventObjective objective)
        {
            if (objective.Status != PublicEventStatus.Succeeded)
                return;

            switch ((PublicEventObjective)objective.Entry.Id)
            {
                case PublicEventObjective.DestroyTheGiantMoodieTotem:// it doesn't end now, it has a delay with some chat, maybe the blank objectives do something
                    publicEvent.Finish(PublicEventTeam.PublicTeam);
                    break;
            }
        }
    }
}
