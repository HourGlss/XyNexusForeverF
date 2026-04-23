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

namespace NexusForever.Script.Instance.Expedition.FragmentZero
{
    [ScriptFilterOwnerId(680)]
    public class FragmentZeroEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;
        private IMapInstance mapInstance;

        private uint gatherMarker1Guid;
        private uint gatherRing1TriggerGuid;
        private uint firstDoorGuid;
        private uint gatherMarker2Guid;


        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;
        private readonly IGlobalQuestManager globalQuestManager;

        public FragmentZeroEventScript(
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

            //publicEvent.SetPhase(PublicEventPhase.SearchForMissingCrew);
            publicEvent.SetPhase(PublicEventPhase.ContinueTheSearchForTheMissingCrew);
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
                case PublicEventCreature.SkeechAmbushRing:
                    gatherMarker1Guid = worldEntity.Guid;
                    break;
                case PublicEventCreature.GatherRing2:
                    gatherMarker2Guid = worldEntity.Guid;
                    break;
            }
        }

        private void OnAddToMapWorldLocationEntity(IWorldLocationVolumeGridTriggerEntity worldLocationEntity)
        {
            switch (worldLocationEntity.Entry.Id)
            {
                case 49225:
                    gatherRing1TriggerGuid = worldLocationEntity.Guid;
                    break;
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
                case PublicEventCreature.SkeechAmbushRing:
                    gatherMarker1Guid = 0;
                    break;
            }
        }

        private void OnRemoveFromMapWorldLocationEntity(IWorldLocationVolumeGridTriggerEntity worldLocationEntity)
        {
            switch (worldLocationEntity.Entry.Id)
            {
                case 49225:
                    gatherRing1TriggerGuid = 0;
                    break;
            }
        }

        /// <summary>
        /// Invoked when the public event phase changes.
        /// </summary>
        public void OnPublicEventPhase(uint phase)
        {
            switch ((PublicEventPhase)phase)
            {
                case PublicEventPhase.SearchForMissingCrew:
                    OnPhaseSearchForMissingCrew();
                    break;
                case PublicEventPhase.FollowTheFriendlySkeech:
                    OnPhaseFollowTheFriendlySkeech();
                     break;
                case PublicEventPhase.SurviveTheSkeechAmbush:
                    OnPhaseSurviveTheSkeechAmbush();
                    break;
                case PublicEventPhase.ContinueTheSearchForTheMissingCrew:
                    OnPhaseContinueTheSearchForTheMissingCrew();
                    break;
                case PublicEventPhase.SearchForCrewmateJo:
                    OnPhaseSearchForCrewmateJo();
                    break;
                case PublicEventPhase.DefeatPrototypes:
                    OnPhaseDefeatPrototypes();
                    break;
                case PublicEventPhase.SearchJoscorpse:
                    OnPhaseSearchJoscorpse();
                    break;
                case PublicEventPhase.ReturnToTheAirlockOfTheIncubationComplex:
                    OnPhaseReturnToTheAirlockOfTheIncubationComplex();
                    break;
                case PublicEventPhase.ContinueTheSearchForTheMissingCrewLifeOverseer:
                    OnPhaseContinueTheSearchForTheMissingCrewLifeOverseer();
                    break;
                case PublicEventPhase.SearchInsideTheBiomaticsChamberForSyrus:
                    OnPhaseSearchInsideTheBiomaticsChamberForSyrus();
                    break;
                case PublicEventPhase.DefeatProjectMatron:
                    OnPhaseDefeatProjectMatron();
                    break;
                case PublicEventPhase.SearchCrewmateSyrusCorpse:
                    OnPhaseSearchCrewmateSyrusCorpse();
                    break;
                case PublicEventPhase.ReturnToTheAirlockOfTheBiomaticsChamber:
                    OnPhaseReturnToTheAirlockOfTheBiomaticsChamber();
                    break;
                case PublicEventPhase.ContinueTheSearchForHugo:
                    OnPhaseContinueTheSearchForHugo();
                    break;
                case PublicEventPhase.SearchForHugo:
                    OnPhaseSearchForHugo();
                    break;
                case PublicEventPhase.DefeatTheLifeOverseer:
                    OnPhaseDefeatTheLifeOverseer();
                    break;
                case PublicEventPhase.LocateHugo:
                    OnPhaseLocateHugo();
                    break;
                case PublicEventPhase.WaitForHugo:
                    OnPhaseWaitForHugo();
                    break;
                case PublicEventPhase.SeeIfHugoHasAWayOutOfThisMess:
                    OnPhaseSeeIfHugoHasAWayOutOfThisMess();
                    break;
                case PublicEventPhase.StayCloseToHugo:
                    OnPhaseStayCloseToHugo();
                    break;
                case PublicEventPhase.SpeakWithHugo:
                    OnPhaseSpeakWithHugo();
                    break;
           }
        }
        private void OnPhaseSearchForMissingCrew()
        {
            var triggerEntity = publicEvent.CreateEntity<ITurnstileGridTriggerEntity>();
            triggerEntity.Initialise(4397, 15f, 4397);
            triggerEntity.AddToMap(mapInstance, new Vector3(9865.48f, -765.58f, -5896.74f)); // get real trigger range and correct coordinates
        }
        private void OnPhaseFollowTheFriendlySkeech()
        {
            publicEvent.ActivateObjective(PublicEventObjective.FollowTheFriendlySkeech, mapInstance.PlayerCount); // move this into phase one

            var triggerEntity = publicEvent.CreateEntity<IWorldLocationVolumeGridTriggerEntity>();
            triggerEntity.Initialise(49225, 7902);
            triggerEntity.AddToMap(mapInstance, new Vector3(9894.729f, -783.358f, -6032.538f));

            BroadcastCommunicatorMessage(CommunicatorMessage.SupervisorLola);
        }

        private void OnPhaseSurviveTheSkeechAmbush()
        {
            publicEvent.ActivateObjective(PublicEventObjective.SurviveTheSkeechAmbush, 11);

            var gatherRing = mapInstance.GetEntity<IWorldEntity>(gatherMarker1Guid);
            gatherRing?.RemoveFromMap();

            var triggerEntity = mapInstance.GetEntity<IWorldLocationVolumeGridTriggerEntity>(gatherRing1TriggerGuid);
            triggerEntity?.RemoveFromMap();

            BroadcastCommunicatorMessage(CommunicatorMessage.SupervisorLolax);
        }

        private void OnPhaseContinueTheSearchForTheMissingCrew()
        {
            publicEvent.ActivateObjective(PublicEventObjective.ContinueTheSearchForTheMissingCrew, mapInstance.PlayerCount);// maybe this is the wrong objective
            publicEvent.ActivateObjective(PublicEventObjective.LocateTheShipsBlackBox);
            publicEvent.ActivateObjective(PublicEventObjective.EliminateSkeech);

            var triggerEntity = publicEvent.CreateEntity<IWorldLocationVolumeGridTriggerEntity>();
            triggerEntity.Initialise(48711, 7903);
            triggerEntity.AddToMap(mapInstance, new Vector3(9685.685f, -767.6072f, -6098.117f));

            foreach (IPlayer player in mapInstance.GetPlayers())
                player.CinematicManager.QueueCinematic(cinematicFactory.CreateCinematic<IFragmentZeroFirstWarning>());

            IDoorEntity door = mapInstance.GetEntity<IDoorEntity>(firstDoorGuid);
            door?.OpenDoor();
        }

        private void OnPhaseSearchForCrewmateJo()
        {
            publicEvent.ActivateObjective(PublicEventObjective.SearchInsideTheIncubationComplexForJo);
            publicEvent.ActivateObjective(PublicEventObjective.SmashXenobiteEggsInsideTheIncubationComplex); // this is dalayed or from trigger

            var gatherRing = mapInstance.GetEntity<IWorldEntity>(gatherMarker2Guid);
            gatherRing?.RemoveFromMap();
        }

        private void OnPhaseDefeatPrototypes()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatPrototypeAlphansideTheIncubationComplex);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatPrototypeBeta);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatPrototypeDelta);
        }

        private void OnPhaseSearchJoscorpse()
        {
            publicEvent.ActivateObjective(PublicEventObjective.SearchJoInsideTheIncubationComplex);
        }

        private void OnPhaseReturnToTheAirlockOfTheIncubationComplex()
        {
            publicEvent.ActivateObjective(PublicEventObjective.ReturnToTheAirlockOfTheIncubationComplex, mapInstance.PlayerCount);
        }

        private void OnPhaseContinueTheSearchForTheMissingCrewLifeOverseer()
        {
            publicEvent.ActivateObjective(PublicEventObjective.ContinueTheSearchForTheMissingCrewLifeOverseer); // maybe, or could be wrong
            publicEvent.ActivateObjective(PublicEventObjective.CollectCargoCrate); // maybe, could be wrong
        }

        private void OnPhaseSearchInsideTheBiomaticsChamberForSyrus()
        {
            publicEvent.ActivateObjective(PublicEventObjective.SearchInsideTheBiomaticsChamberForSyrus);
            publicEvent.ActivateObjective(PublicEventObjective.DeactivateAutomatedDefences);
        }

        private void OnPhaseDefeatProjectMatron()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatProjectMatron);
        }

        private void OnPhaseSearchCrewmateSyrusCorpse()
        {
            publicEvent.ActivateObjective(PublicEventObjective.SearchCrewmateSyrusCorpse);
        }

        private void OnPhaseReturnToTheAirlockOfTheBiomaticsChamber()
        {
            publicEvent.ActivateObjective(PublicEventObjective.ReturnToTheAirlockOfTheBiomaticsChamber, mapInstance.PlayerCount);
        }

        private void OnPhaseContinueTheSearchForHugo()
        {
            publicEvent.ActivateObjective(PublicEventObjective.ContinueTheSearchForHugo, mapInstance.PlayerCount);
        }

        private void OnPhaseSearchForHugo()
        {
            publicEvent.ActivateObjective(PublicEventObjective.SearchForHugo, mapInstance.PlayerCount);
        }

        private void OnPhaseDefeatTheLifeOverseer()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheLifeOverseer);
        }

        private void OnPhaseLocateHugo()
        {
            publicEvent.ActivateObjective(PublicEventObjective.LocateHugo);
        }

        private void OnPhaseWaitForHugo()
        {
            publicEvent.ActivateObjective(PublicEventObjective.WaitForHugo);
        }

        private void OnPhaseSeeIfHugoHasAWayOutOfThisMess()
        {
            publicEvent.ActivateObjective(PublicEventObjective.SeeIfHugoHasAWayOutOfThisMess);
        }

        private void OnPhaseStayCloseToHugo()
        {
            publicEvent.ActivateObjective(PublicEventObjective.StayCloseToHugo);
        }

        private void OnPhaseSpeakWithHugo()
        {
            publicEvent.ActivateObjective(PublicEventObjective.SpeakWithHugo);
        }

        private void BroadcastCommunicatorMessage(CommunicatorMessage message)
        {
            ICommunicatorMessage communicatorMessage = globalQuestManager.GetCommunicatorMessage(message);
            foreach (IPlayer player in mapInstance.GetPlayers())
                communicatorMessage?.Send(player.Session);
        }

        private void SendCommunicatorMessage(IPlayer player, CommunicatorMessage message)
        {
            ICommunicatorMessage communicatorMessage = globalQuestManager.GetCommunicatorMessage(message);
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
                case PublicEventObjective.FollowTheFriendlySkeech:
                    publicEvent.SetPhase(PublicEventPhase.SurviveTheSkeechAmbush);
                    break;
                case PublicEventObjective.SurviveTheSkeechAmbush:
                    publicEvent.SetPhase(PublicEventPhase.ContinueTheSearchForTheMissingCrew);
                    break;
                case PublicEventObjective.ContinueTheSearchForTheMissingCrew:
                    publicEvent.SetPhase(PublicEventPhase.SearchForCrewmateJo);
                    break;
                case PublicEventObjective.SearchInsideTheIncubationComplexForJo:
                    publicEvent.SetPhase(PublicEventPhase.DefeatPrototypes);
                    break;
                case PublicEventObjective.DefeatPrototypeAlphansideTheIncubationComplex:
                case PublicEventObjective.DefeatPrototypeBeta:
                case PublicEventObjective.DefeatPrototypeDelta:
                    publicEvent.SetPhase(PublicEventPhase.SearchJoscorpse);
                    break;
                case PublicEventObjective.SearchJoInsideTheIncubationComplex:
                    publicEvent.SetPhase(PublicEventPhase.ReturnToTheAirlockOfTheIncubationComplex);
                    break;
                case PublicEventObjective.ReturnToTheAirlockOfTheIncubationComplex:
                    publicEvent.SetPhase(PublicEventPhase.ContinueTheSearchForTheMissingCrewLifeOverseer);
                    break;
                case PublicEventObjective.ContinueTheSearchForTheMissingCrewLifeOverseer:
                    publicEvent.SetPhase(PublicEventPhase.SearchInsideTheBiomaticsChamberForSyrus);
                    break;
                case PublicEventObjective.SearchInsideTheBiomaticsChamberForSyrus:
                    publicEvent.SetPhase(PublicEventPhase.DefeatProjectMatron);
                    break;
                case PublicEventObjective.DefeatProjectMatron:
                    publicEvent.SetPhase(PublicEventPhase.SearchCrewmateSyrusCorpse);
                    break;
                case PublicEventObjective.SearchCrewmateSyrusCorpse:
                    publicEvent.SetPhase(PublicEventPhase.ReturnToTheAirlockOfTheBiomaticsChamber);
                    break;
                case PublicEventObjective.ReturnToTheAirlockOfTheBiomaticsChamber:
                    publicEvent.SetPhase(PublicEventPhase.SearchForHugo);
                    break;
                case PublicEventObjective.SearchForHugo:
                    publicEvent.SetPhase(PublicEventPhase.DefeatTheLifeOverseer);
                    break;
                case PublicEventObjective.DefeatTheLifeOverseer:
                    publicEvent.SetPhase(PublicEventPhase.LocateHugo);
                    break;
                case PublicEventObjective.LocateHugo:
                    publicEvent.SetPhase(PublicEventPhase.WaitForHugo);
                    break;
                case PublicEventObjective.WaitForHugo:
                    publicEvent.SetPhase(PublicEventPhase.SeeIfHugoHasAWayOutOfThisMess);
                    break;
                case PublicEventObjective.SeeIfHugoHasAWayOutOfThisMess:
                    publicEvent.SetPhase(PublicEventPhase.StayCloseToHugo);
                    break;
                case PublicEventObjective.StayCloseToHugo:
                    publicEvent.SetPhase(PublicEventPhase.SpeakWithHugo);
                    break;
                case PublicEventObjective.SpeakWithHugo:
                    publicEvent.Finish(PublicEventTeam.PublicTeam);
                    break;
            }
        }

        /// <summary>
        /// Invoked when a cinematic for <see cref="IPlayer"/> has finished.
        /// </summary>
        public void OnCinematicFinish(IPlayer player, uint cinematicId)
        {
            switch ((PublicEventPhase)publicEvent.Phase)
            {
                case PublicEventPhase.ContinueTheSearchForTheMissingCrew:
                    SendCommunicatorMessage(player, CommunicatorMessage.CaptainHugo1);
                    break;
            }
        }
    }
}
