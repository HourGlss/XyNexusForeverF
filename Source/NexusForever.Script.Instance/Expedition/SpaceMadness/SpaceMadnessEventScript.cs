using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Cinematic.Cinematics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Trigger;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Expedition.SpaceMadness
{
    [ScriptFilterOwnerId(390)]
    public class SpaceMadnessEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;
        private IMapInstance mapInstance;

        private uint gatherRingGuid;

        private uint firstDoorGuid;
        private uint secondDoorGuid;
        private uint airLockDoorGuid;
        private uint airLockExitDoorGuid;

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;
        private readonly IGlobalQuestManager globalQuestManager;

        public SpaceMadnessEventScript(
            ICinematicFactory cinematicFactory,
            IGlobalQuestManager globalQuestManager)
        {
            this.cinematicFactory   = cinematicFactory;
            this.globalQuestManager = globalQuestManager;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IPublicEvent owner)
        {
            publicEvent = owner;
            mapInstance = publicEvent.Map as IMapInstance;

            publicEvent.SetPhase(PublicEventPhase.TalkToCaptainTero);
        }

        /// <summary>
        /// Invoked when a <see cref="IGridEntity"/> is added to the map the public event is on.
        /// </summary>
        public void OnAddToMap(IGridEntity entity)
        {
            switch (entity)
            {
                case IWorldEntity worldEntity:
                    OnAddToMapWorldEntity(worldEntity);
                    break;
                case IWorldLocationVolumeGridTriggerEntity worldLocationEntity:
                    OnAddToMapWorldLocationEntity(worldLocationEntity);
                    break;
            }
        }

        private void OnAddToMapWorldEntity(IWorldEntity worldEntity)
        {
            switch ((PublicEventCreature)worldEntity.CreatureId)
            {
                case PublicEventCreature.FirstDoor:
                    firstDoorGuid = worldEntity.Guid;
                    break;
                case PublicEventCreature.SecondDoor:
                    secondDoorGuid = worldEntity.Guid;
                    break;
                case PublicEventCreature.AirLockDoor:
                    airLockDoorGuid = worldEntity.Guid;
                    break;
            }
        }

        private void OnAddToMapWorldLocationEntity(IWorldLocationVolumeGridTriggerEntity worldLocationEntity)
        {
            switch (worldLocationEntity.Entry.Id)
            {

            }
        }

        /// <summary>
        /// Invoked when a <see cref="IGridEntity"/> is removed from the map the public event is on.
        /// </summary>
        public void OnRemoveFromMap(IGridEntity entity)
        {
            switch (entity)
            {
                case IWorldEntity worldEntity:
                    OnRemoveFromMapWorldEntity(worldEntity);
                    break;
                case IWorldLocationVolumeGridTriggerEntity worldLocationEntity:
                    OnRemoveFromMapWorldLocationEntity(worldLocationEntity);
                    break;
            }
        }

        private void OnRemoveFromMapWorldEntity(IWorldEntity worldEntity)
        {
            switch ((PublicEventCreature)worldEntity.CreatureId)
            {

            }
        }

        private void OnRemoveFromMapWorldLocationEntity(IWorldLocationVolumeGridTriggerEntity worldLocationEntity)
        {
            switch (worldLocationEntity.Entry.Id)
            {

            }
        }

        /// <summary>
        /// Invoked when the public event phase changes.
        /// </summary>
        public void OnPublicEventPhase(uint phase)
        {
            switch ((PublicEventPhase)phase)
            {
                case PublicEventPhase.TalkToCaptainTero:
                    OnPhaseTalkToCaptainTero();
                    break;
                case PublicEventPhase.TalkToMajorLeeBarmy:
                    OnPhaseTalkToMajorLeeBarmy();
                    break;
                case PublicEventPhase.EnterTheAirlock:
                    OnPhaseEnterTheAirlock();
                    break;
                case PublicEventPhase.AccessTheObservationDeckComputer:
                    OnPhaseAccessTheObservationDeckComputer();
                    break;
                case PublicEventPhase.EnterTheResearchLaboratory:
                    OnPhaseEnterTheResearchLaboratory();
                    break;
                case PublicEventPhase.OpenHazmatStorageCloset:
                    OnPhaseOpenHazmatStorageCloset();
                    break;
                case PublicEventPhase.SurviveYourNightmares:
                    OnPhaseSurviveYourNightmares();
                    break;
                case PublicEventPhase.EquipAHazmatSuit:
                    OnPhaseEquipAHazmatSuit();
                    break;
                case PublicEventPhase.KillHallucinatingLivestock:
                    OnPhaseKillHallucinatingLivestock();
                    break;
                case PublicEventPhase.ActivateAirScrubberControls:
                    OnPhaseActivateAirScrubberControls();
                    break;
                case PublicEventPhase.SurviveTheAirScrubbingProcess:
                    OnPhaseSurviveTheAirScrubbingProcess();
                    break;
                case PublicEventPhase.SendTheAllClearSignal:
                    OnPhaseSendTheAllClearSignal();
                    break;
            }
        }

        private void OnPhaseTalkToCaptainTero()
        {
            IDoorEntity door = mapInstance.GetEntity<IDoorEntity>(firstDoorGuid);
            door?.OpenDoor();
        }

        private void OnPhaseTalkToMajorLeeBarmy()
        {
            publicEvent.ActivateObjective(PublicEventObjective.TalkToMajorLeeBarmy);

            IDoorEntity door = mapInstance.GetEntity<IDoorEntity>(secondDoorGuid);
            door?.OpenDoor();
        }

        private void OnPhaseEnterTheAirlock()
        {
            publicEvent.ActivateObjective(PublicEventObjective.EnterTheAirlock, mapInstance.PlayerCount);

            IDoorEntity door = mapInstance.GetEntity<IDoorEntity>(airLockDoorGuid);
            door?.OpenDoor();

            var triggerEntity = publicEvent.CreateEntity<IWorldLocationVolumeGridTriggerEntity>();
            triggerEntity.Initialise(37702, 5512);
            triggerEntity.AddToMap(mapInstance, new Vector3(-35.28216f, 4.942017f, 270.0408f));// place trigger correctly
        }

        private void OnPhaseAccessTheObservationDeckComputer()
        {
            publicEvent.ActivateObjective(PublicEventObjective.AccessTheObservationDeckComputer);
            publicEvent.ActivateObjective(PublicEventObjective.SavePanickedWorkers);
            publicEvent.ActivateObjective(PublicEventObjective.AccessCrewDatapads);
            publicEvent.ActivateObjective(PublicEventObjective.CollectEscapedCreatures, 3);// I am not sure how to do this

            IDoorEntity door = mapInstance.GetEntity<IDoorEntity>(airLockExitDoorGuid); // look on YT when to open this
            door?.OpenDoor();
        }

        private void OnPhaseEnterTheResearchLaboratory()
        {
            publicEvent.ActivateObjective(PublicEventObjective.EnterTheResearchLaboratory, mapInstance.PlayerCount);

            var triggerEntity = publicEvent.CreateEntity<IWorldLocationVolumeGridTriggerEntity>();
            triggerEntity.Initialise(37621, 5531);
            triggerEntity.AddToMap(mapInstance, new Vector3(-55.08974f, -0.05234623f, 137.5821f)); // place trigger correctly
        }

        private void OnPhaseOpenHazmatStorageCloset()
        {
            publicEvent.ActivateObjective(PublicEventObjective.OpenHazmatStorageCloset);
        }

        private void OnPhaseSurviveYourNightmares()
        {
            publicEvent.ActivateObjective(PublicEventObjective.SurviveYourNightmares);

            foreach (IPlayer player in mapInstance.GetPlayers())
                player.TeleportToLocal(new Vector3(0.5417912f, -2.837262f, 119.9983f));//call a world location ID 37993

            //TODO: display text on screen (431194)
        }

        private void OnPhaseEquipAHazmatSuit()
        {
            publicEvent.ActivateObjective(PublicEventObjective.EquipAHazmatSuit);
        }

        private void OnPhaseKillHallucinatingLivestock()
        {
            publicEvent.ActivateObjective(PublicEventObjective.KillHallucinatingLivestock);
            publicEvent.ActivateObjective(PublicEventObjective.GiveAirHelmsToWorkers);
            publicEvent.ActivateObjective(PublicEventObjective.AvoidExplodingRowsdowers);
        }

        private void OnPhaseSurviveTheAirScrubbingProcess()
        {
            publicEvent.ActivateObjective(PublicEventObjective.SurviveTheAirScrubbingProcess);
        }

        private void OnPhaseActivateAirScrubberControls()
        {
            publicEvent.ActivateObjective(PublicEventObjective.ActivateAirScrubberControls);
        }

        private void OnPhaseSendTheAllClearSignal()
        {
            publicEvent.ActivateObjective(PublicEventObjective.SendTheAllClearSignal);
        }

        private void BroadcastCommunicatorMessage(CommunicatorMessage message)
        {
            ICommunicatorMessage communicatorMessage = globalQuestManager.GetCommunicatorMessage(message);
            foreach (IPlayer player in mapInstance.GetPlayers())
                communicatorMessage?.Send(player.Session);
        }

        /// <summary>
        /// Invoked when the <see cref="IPublicEventObjective"/> status changes.
        /// </summary>
        public void OnPublicEventObjectiveStatus(IPublicEventObjective objective)
        {
            if (objective.Status != PublicEventStatus.Succeeded)
                return;

            switch ((PublicEventObjective)objective.Entry.Id)
            {
                case PublicEventObjective.TalkToCaptainTero:
                    publicEvent.SetPhase(PublicEventPhase.TalkToMajorLeeBarmy);
                    break;
                case PublicEventObjective.TalkToMajorLeeBarmy:
                    publicEvent.SetPhase(PublicEventPhase.EnterTheAirlock);
                    break;
                case PublicEventObjective.EnterTheAirlock:
                    publicEvent.SetPhase(PublicEventPhase.AccessTheObservationDeckComputer);
                    break;
                case PublicEventObjective.AccessTheObservationDeckComputer:
                    publicEvent.SetPhase(PublicEventPhase.EnterTheResearchLaboratory);
                    break;
                case PublicEventObjective.EnterTheResearchLaboratory:
                    publicEvent.SetPhase(PublicEventPhase.OpenHazmatStorageCloset);
                    break;
                case PublicEventObjective.OpenHazmatStorageCloset:
                    publicEvent.SetPhase(PublicEventPhase.SurviveYourNightmares);
                    break;
                case PublicEventObjective.SurviveYourNightmares:
                    publicEvent.SetPhase(PublicEventPhase.EquipAHazmatSuit);
                    break;
                case PublicEventObjective.EquipAHazmatSuit:
                    publicEvent.SetPhase(PublicEventPhase.KillHallucinatingLivestock);
                    break;
                case PublicEventObjective.KillHallucinatingLivestock:
                    publicEvent.SetPhase(PublicEventPhase.ActivateAirScrubberControls);
                    break;
                case PublicEventObjective.ActivateAirScrubberControls:
                    publicEvent.SetPhase(PublicEventPhase.SurviveTheAirScrubbingProcess);
                    break;
                case PublicEventObjective.SurviveTheAirScrubbingProcess:
                    publicEvent.SetPhase(PublicEventPhase.SendTheAllClearSignal);
                    break;
                case PublicEventObjective.SendTheAllClearSignal:
                    publicEvent.Finish(PublicEventTeam.PublicTeam);
                    break;
            }
        }
    }
}
