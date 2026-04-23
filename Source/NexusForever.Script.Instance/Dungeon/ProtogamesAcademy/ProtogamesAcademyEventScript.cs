using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Trigger;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Dungeon.ProtogamesAcademy
{
    [ScriptFilterOwnerId(667)]
    public class ProtogamesAcademyEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;
        private IMapInstance mapInstance;

        private uint gatherObjectiveGuid;
        private uint gatherInvulnotronTriggerGuid;
        private uint gatherGromkaTriggerGuid;
        private uint room1PortalTriggerGuid;

        private uint room1PlatformGuid;
        private uint room1LauncherGuid;
        private uint room1Launcher2Guid;
        private uint room1PortalGuid;

        private enum Creature
        {
            PhineasARotostar = 68342,
            PhineasPlatform  = 68372
        }

        #region Dependency Injection

        private readonly IGlobalQuestManager globalQuestManager;

        public ProtogamesAcademyEventScript(
            IGlobalQuestManager globalQuestManager)
        {
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

            //publicEvent.SetPhase(PublicEventPhase.InitiateProtogamesAcademy);
            publicEvent.SetPhase(PublicEventPhase.DefeatInvulnotron);
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
                case PublicEventCreature.Room1Platform:
                    room1PlatformGuid = worldEntity.Guid;
                    break;
                case PublicEventCreature.GatherObjective:
                    gatherObjectiveGuid = worldEntity.Guid;
                    break;
                case PublicEventCreature.Room1Launcher:
                    room1LauncherGuid = worldEntity.Guid;
                    break;
                case PublicEventCreature.Room1Portal:
                    room1PortalGuid = worldEntity.Guid;
                    break;
              /*case PublicEventCreature.Room1Launcher:
                    OnAddToMapLauncher(worldEntity);
                    break;*/
            }
        }

        private void OnAddToMapWorldLocationEntity(IWorldLocationVolumeGridTriggerEntity worldLocationEntity)
        {
            switch (worldLocationEntity.Entry.Id)
            {
                case 48842:
                    gatherInvulnotronTriggerGuid = worldLocationEntity.Guid;
                    break;
                case 48843:
                    gatherGromkaTriggerGuid = worldLocationEntity.Guid;
                    break;
                case 50164:
                     room1PortalTriggerGuid = worldLocationEntity.Guid;
                    break;
            }
        }

        private void OnAddToMapLauncher(IWorldEntity entity)
        {
            switch (entity.ActivePropId)
            {
                case 1://get real ID
                    room1LauncherGuid = entity.Guid;
                    break;
                case 2://get real ID
                    room1Launcher2Guid = entity.Guid;
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
                case PublicEventCreature.Room1Platform:
                    room1PlatformGuid = 0;
                    break;
                case PublicEventCreature.GatherObjective:
                    gatherObjectiveGuid = 0;
                    break;
                case PublicEventCreature.Room1Launcher:
                    room1LauncherGuid = 0;
                    break;
            }
        }

        private void OnRemoveFromMapWorldLocationEntity(IWorldLocationVolumeGridTriggerEntity worldLocationEntity)
        {
            switch (worldLocationEntity.Entry.Id)
            {
                case 48842:
                    gatherInvulnotronTriggerGuid = 0;
                    break;
                case 48843:
                    gatherGromkaTriggerGuid = 0;
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
                case PublicEventPhase.GatherInvulnotron:
                    OnPhaseGatherInvulnotron();
                    break;
                case PublicEventPhase.DefeatInvulnotron:
                    OnPhaseDefeatInvulnotron();
                    break;
                case PublicEventPhase.GatherGromka:
                    OnPhaseGatherGromka();
                    break;
                case PublicEventPhase.DefeatGromka:
                    OnPhaseDefeatGromka();
                    break;
                case PublicEventPhase.GatherIrukiBoldbeard:
                    OnPhaseGatherIrukiBoldbeard();
                    break;
                case PublicEventPhase.DefeatIrukiBoldbeard:
                    OnPhaseDefeatIrukiBoldbeard();
                    break;
                case PublicEventPhase.TeleporterToTheNextEvent:
                    OnPhaseteleporterToTheNextEvent();
                    break;
                case PublicEventPhase.DefeatSeekNSlaughter:
                    OnPhaseDefeatSeekNSlaughter();
                    break;
                case PublicEventPhase.Gather:
                    OnPhaseGather();
                    break;
                case PublicEventPhase.DefeatIceboxMk2:
                    OnPhaseDefeatIceboxMk2();
                    break;
                case PublicEventPhase.Gather2:
                    OnPhaseGather2();
                    break;
                case PublicEventPhase.DefeatSuperInvulnotron:
                    OnPhaseDefeatSuperInvulnotron();
                    break;
                case PublicEventPhase.GoToLastEvent:
                    OnPhaseGoToLastEvent();
                    break;
                case PublicEventPhase.MeetWithPhineasARotostar:
                    OnPhaseMeetWithPhineasARotostar();
                    break;
                case PublicEventPhase.DefeatWrathbone:
                    OnPhaseDefeatWrathbone();
                    break;
            }
        }

        private void OnPhaseGatherInvulnotron()
        {
            publicEvent.ActivateObjective(PublicEventObjective.GatherInvulnotron, mapInstance.PlayerCount);

            var triggerEntity = publicEvent.CreateEntity<IWorldLocationVolumeGridTriggerEntity>();
            triggerEntity.Initialise(48842, 7932);
            triggerEntity.AddToMap(mapInstance, new Vector3(-24400.1f, -974.668f, -28977.1f));
        }

        private void OnPhaseDefeatInvulnotron()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatInvulnotron);

            var triggerEntity = mapInstance.GetEntity<IWorldLocationVolumeGridTriggerEntity>(gatherInvulnotronTriggerGuid);
            triggerEntity?.RemoveFromMap();

            var gatherRing = mapInstance.GetEntity<IWorldEntity>(gatherObjectiveGuid);
            gatherRing?.RemoveFromMap();
        }

        private void OnPhaseGatherGromka()
        {
            publicEvent.ActivateObjective(PublicEventObjective.GatherGromka, mapInstance.PlayerCount);

            BroadcastCommunicatorMessage(CommunicatorMessage.PhineasARotostar1);

            var triggerEntity = publicEvent.CreateEntity<IWorldLocationVolumeGridTriggerEntity>();
            triggerEntity.Initialise(48843, 7933);
            triggerEntity.AddToMap(mapInstance, new Vector3(-24504.9f, -974.749f, -28857.5f));
        }

        private void OnPhaseDefeatGromka()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatGromka);

            BroadcastCommunicatorMessage(CommunicatorMessage.PhineasARotostar2);

            var triggerEntity = mapInstance.GetEntity<IWorldLocationVolumeGridTriggerEntity>(gatherGromkaTriggerGuid);
            triggerEntity?.RemoveFromMap();

            var gatherRingEntity = mapInstance.GetEntity<IWorldEntity>(gatherObjectiveGuid);
            gatherRingEntity?.RemoveFromMap();

            var platformEntity = mapInstance.GetEntity<IWorldEntity>(room1PlatformGuid);
            platformEntity?.RemoveFromMap();

            var LauncherEntity = mapInstance.GetEntity<IWorldEntity>(room1LauncherGuid);
            LauncherEntity?.RemoveFromMap();
        }

        private void OnPhaseGatherIrukiBoldbeard()
        {
            publicEvent.ActivateObjective(PublicEventObjective.GatherIrukiBoldbeard, mapInstance.PlayerCount);

            BroadcastCommunicatorMessage(CommunicatorMessage.PhineasARotostar3);

            var triggerEntity = publicEvent.CreateEntity<IWorldLocationVolumeGridTriggerEntity>();
            triggerEntity.Initialise(48842, 7932);
            triggerEntity.AddToMap(mapInstance, new Vector3(-24400.1f, -974.668f, -28977.1f));
        }
        private void OnPhaseDefeatIrukiBoldbeard()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatIrukiBoldbeard);

            BroadcastCommunicatorMessage(CommunicatorMessage.PhineasARotostar4);

            var triggerEntity = mapInstance.GetEntity<IWorldLocationVolumeGridTriggerEntity>(gatherInvulnotronTriggerGuid); // both us the same World location
            triggerEntity?.RemoveFromMap();

            var gatherRing = mapInstance.GetEntity<IWorldEntity>(gatherObjectiveGuid);
            gatherRing?.RemoveFromMap();

            var LauncherEntity = mapInstance.GetEntity<IWorldEntity>(room1Launcher2Guid);
            LauncherEntity?.RemoveFromMap();
        }

        private void OnPhaseteleporterToTheNextEvent()
        {
            publicEvent.ActivateObjective(PublicEventObjective.TeleporterToTheNextEvent1);

            BroadcastCommunicatorMessage(CommunicatorMessage.PhineasARotostar5);

            var triggerEntity = publicEvent.CreateEntity<IWorldLocationVolumeGridTriggerEntity>();
            triggerEntity.Initialise(50164, 7936);
            triggerEntity.AddToMap(mapInstance, new Vector3(-24400.19f, -974.668f, -28977.15f));
        }

        private void OnPhaseDefeatSeekNSlaughter()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatSeekNSlaughter);
        }

        private void OnPhaseGather()
        {
            BroadcastCommunicatorMessage(CommunicatorMessage.PhineasARotostar6);

            publicEvent.ActivateObjective(PublicEventObjective.Gather, mapInstance.PlayerCount);//maybe I got this wrong

            var triggerEntity = publicEvent.CreateEntity<IWorldLocationVolumeGridTriggerEntity>();
            triggerEntity.Initialise(48846, 7934);
            triggerEntity.AddToMap(mapInstance, new Vector3(-19804.34f, -945.5437f, -29483.94f));
        }

        private void OnPhaseDefeatIceboxMk2()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatIceboxMk2);

            BroadcastCommunicatorMessage(CommunicatorMessage.PhineasARotostar7);
        }

        private void OnPhaseGather2()
        {
            publicEvent.ActivateObjective(PublicEventObjective.Gather2, mapInstance.PlayerCount);

            BroadcastCommunicatorMessage(CommunicatorMessage.PhineasARotostar8);

            var triggerEntity = publicEvent.CreateEntity<IWorldLocationVolumeGridTriggerEntity>();
            triggerEntity.Initialise(48846, 7934);
            triggerEntity.AddToMap(mapInstance, new Vector3(-19804.34f, -945.5437f, -29483.94f));
        }

        private void OnPhaseDefeatSuperInvulnotron()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatSuperInvulnotron);
        }

        private void OnPhaseGoToLastEvent()
        {
            publicEvent.ActivateObjective(PublicEventObjective.GoToTheLastEvent, mapInstance.PlayerCount);

            BroadcastCommunicatorMessage(CommunicatorMessage.PhineasARotostar9);
        }

        private void OnPhaseMeetWithPhineasARotostar()
        {
            publicEvent.ActivateObjective(PublicEventObjective.MeetWithPhineasARotostar, mapInstance.PlayerCount);
        }

        private void OnPhaseDefeatWrathbone()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatWrathbone);
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
                case PublicEventObjective.InitiateProtogamesAcademy:
                    publicEvent.SetPhase(PublicEventPhase.GatherInvulnotron);
                    break;
                case PublicEventObjective.GatherInvulnotron:
                    publicEvent.SetPhase(PublicEventPhase.DefeatInvulnotron);
                    break;
                case PublicEventObjective.DefeatInvulnotron:
                    publicEvent.SetPhase(PublicEventPhase.GatherGromka);
                    break;
                case PublicEventObjective.GatherGromka:
                    publicEvent.SetPhase(PublicEventPhase.DefeatGromka);
                    break;
                case PublicEventObjective.DefeatGromka:
                    publicEvent.SetPhase(PublicEventPhase.GatherIrukiBoldbeard);
                    break;
                case PublicEventObjective.GatherIrukiBoldbeard:
                    publicEvent.SetPhase(PublicEventPhase.DefeatIrukiBoldbeard);
                    break;
                case PublicEventObjective.DefeatIrukiBoldbeard:
                    publicEvent.SetPhase(PublicEventPhase.TeleporterToTheNextEvent);
                    break;
                case PublicEventObjective.TeleporterToTheNextEvent1:
                    publicEvent.SetPhase(PublicEventPhase.DefeatSeekNSlaughter);
                    break;
                case PublicEventObjective.DefeatSeekNSlaughter:
                    publicEvent.SetPhase(PublicEventPhase.Gather);
                    break;
                case PublicEventObjective.Gather:// is this the correct one?
                    publicEvent.SetPhase(PublicEventPhase.DefeatIceboxMk2);
                    break;
                case PublicEventObjective.DefeatIceboxMk2:
                    publicEvent.SetPhase(PublicEventPhase.Gather2);//idk if I need to flip them
                    break;
                case PublicEventObjective.Gather2:
                    publicEvent.SetPhase(PublicEventPhase.DefeatSuperInvulnotron);
                    break;
                case PublicEventObjective.DefeatSuperInvulnotron:
                    publicEvent.SetPhase(PublicEventPhase.GoToLastEvent);
                    break;
                case PublicEventObjective.GoToTheLastEvent:
                    publicEvent.SetPhase(PublicEventPhase.MeetWithPhineasARotostar);
                    break;
                case PublicEventObjective.MeetWithPhineasARotostar:
                    publicEvent.SetPhase(PublicEventPhase.DefeatWrathbone);
                    break;
                case PublicEventObjective.DefeatWrathbone:
                    publicEvent.Finish(PublicEventTeam.PublicTeam);
                    break;
            }
        }
    }
}
