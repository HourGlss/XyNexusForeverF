using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Game.Abstract.Cinematic.Cinematics;
using NexusForever.Game.Abstract.Entity.Trigger;
using System.Numerics;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Static.PublicEvent;

namespace NexusForever.Script.Instance.Raid.InitializationCoreY83
{
    [ScriptFilterOwnerId(595)]
    public class InitializationCoreY83EventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;
        private IMapInstance mapInstance;

        private uint bossDoorGuid;
        private uint mainDoorGuid;

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;
        private readonly IGlobalQuestManager globalQuestManager;

        public InitializationCoreY83EventScript(
            ICinematicFactory cinematicFactory,
            IGlobalQuestManager globalQuestManager)
        {
            this.cinematicFactory = cinematicFactory;
            this.globalQuestManager = globalQuestManager;
        }

        #endregion

        public void OnLoad(IPublicEvent owner)
        {
            publicEvent = owner;
            publicEvent.SetPhase(PublicEventPhase.Enter);

            mapInstance = publicEvent.Map as IMapInstance;
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
                case PublicEventCreature.BossQuarantineDoor:
                    bossDoorGuid = worldEntity.Guid;
                    break;
                case PublicEventCreature.MainQuarantineDoor:
                    mainDoorGuid = worldEntity.Guid;
                    break;
            }
        }
        public void OnPublicEventPhase(uint phase)
       {

            switch ((PublicEventPhase)phase)
            {
                case PublicEventPhase.Enter:
                    PhaseEnter();
                    break;
                case PublicEventPhase.OpenDoor:
                    PhaseOpenDoor();
                    break;
                case PublicEventPhase.Boss:
                    PhaseBoss();
                    break;
            }
        }
        private void PhaseEnter()
        {
            IDoorEntity door = mapInstance.GetEntity<IDoorEntity>(mainDoorGuid);
            door?.OpenDoor();

            //Look at notes in CentralAccessCorridorTrigger.cs
            var triggerEntity = publicEvent.CreateEntity<ITurnstileGridTriggerEntity>();
            triggerEntity.Initialise(2681, 15f, 2681);//get real range
            triggerEntity.AddToMap(mapInstance, new Vector3(1268.54f, -765.40f, 1271.93f));//get real coordinates
        }

        private void PhaseOpenDoor()
        {
            publicEvent.ActivateObjective(PublicEventObjective.OpenTheQuarantineDoor);

            BroadcastCommunicatorMessage(CommunicatorMessage.Nurton2);

            IDoorEntity door = mapInstance.GetEntity<IDoorEntity>(bossDoorGuid);
            door?.OpenDoor();//maybe move this to the creature2 script
        }

        private void PhaseBoss()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatThePrimeEvolutionaryOperants);
            publicEvent.ActivateObjective(PublicEventObjective.ImmortalPrimeEvolutionaryOperants);
            publicEvent.ActivateObjective(PublicEventObjective.EvolutionaryRevolutions);
            //all 3 are added before the boss is pulled, as of the last patch

            foreach (IPlayer player in mapInstance.GetPlayers())
                player.CinematicManager.QueueCinematic(cinematicFactory.CreateCinematic<IInitializationCoreY83OpenDoor>());
        }

        private void BroadcastCommunicatorMessage(CommunicatorMessage message)
        {
            ICommunicatorMessage communicatorMessage = globalQuestManager.GetCommunicatorMessage(message);
            foreach (IPlayer player in mapInstance.GetPlayers())
                communicatorMessage?.Send(player.Session);
        }

        public void OnPublicEventObjectiveStatus(IPublicEventObjective objective)
        {
            if (objective.Status != PublicEventStatus.Succeeded)
                return;

            switch ((PublicEventObjective)objective.Entry.Id)
            {
                case PublicEventObjective.KillThePhagetouchedFreebots:
                case PublicEventObjective.UnlockTheQuarantineDoor:
                    publicEvent.SetPhase(PublicEventPhase.OpenDoor);
                    break;
                case PublicEventObjective.OpenTheQuarantineDoor:
                    publicEvent.SetPhase(PublicEventPhase.Boss);
                    break;
                case PublicEventObjective.DefeatThePrimeEvolutionaryOperants:
                    {
                        BroadcastCommunicatorMessage(CommunicatorMessage.Nurton3);
                        publicEvent.Finish(PublicEventTeam.PublicTeam);
                    }
                    break;
                //the ramp changes colors once the boss is killed, from techophage color to green
               //Not sure what script to handle it, maybe map script
            }
        }
    }
}
