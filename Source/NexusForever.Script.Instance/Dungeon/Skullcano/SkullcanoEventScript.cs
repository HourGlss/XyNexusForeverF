using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Trigger;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Dungeon.Skullcano
{
    [ScriptFilterOwnerId(148)]
    public class SkullcanoEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;
        private IMapInstance mapInstance;

        private uint CaveBranchDoorGuid;
        private uint ChasmBranchDoorGuid;
        private uint CaveDoorGuid;
        private uint PlatformGuid;

        private uint FindChiefGridTriggerGuid;
        private uint PlatformTriggerGuid;

        #region Dependency Injection

        private readonly IGlobalQuestManager globalQuestManager;

        public SkullcanoEventScript(
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

            publicEvent.SetPhase(PublicEventPhase.Enter);
        }

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
                case PublicEventCreature.CaveBranchDoor:
                    CaveBranchDoorGuid = worldEntity.Guid;
                    break;
                case PublicEventCreature.ChasmBranchDoor:
                    ChasmBranchDoorGuid = worldEntity.Guid;
                    break;
                case PublicEventCreature.CaveDoor:
                    CaveDoorGuid = worldEntity.Guid;
                    break;
                case PublicEventCreature.Platform:
                    PlatformGuid = worldEntity.Guid;
                    break;
            }
        }

        private void OnAddToMapWorldLocationEntity(IWorldLocationVolumeGridTriggerEntity worldLocationEntity)
        {
            switch (worldLocationEntity.Entry.Id)
            {
                case 25365:
                    PlatformTriggerGuid = worldLocationEntity.Guid;
                    break;
                case 33452:
                    FindChiefGridTriggerGuid = worldLocationEntity.Guid;
                    break;
            }
        }

        public void OnRemoveFromMap(IGridEntity entity)
        {
            switch (entity)
            {
                case IWorldLocationVolumeGridTriggerEntity worldLocationEntity:
                    OnRemoveFromMapWorldLocationEntity(worldLocationEntity);
                    break;
            }
        }

        private void OnRemoveFromMapWorldLocationEntity(IWorldLocationVolumeGridTriggerEntity worldLocationEntity)
        {
            switch (worldLocationEntity.Entry.Id)
            {
                case 33452:
                    FindChiefGridTriggerGuid = 0;
                    break;
                case 25365:
                    PlatformTriggerGuid = 0;
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
                case PublicEventPhase.RandomPath:
                    PhaseRandomPath();
                    break;
                case PublicEventPhase.RandomPathFindChief:
                    PhaseRandomPathFindChief();
                    break;
                case PublicEventPhase.RandomPathCave:
                    PhaseRandomPathCave();
                    break;
                case PublicEventPhase.RandomPathChasm:
                    PhaseRandomPathChasm();
                    break;
                case PublicEventPhase.Bosun:
                    PhaseBosun();
                    break;
                case PublicEventPhase.Platform:
                    PhasePlatform();
                    break;
                case PublicEventPhase.GetToRedmoon:
                    PhaseGetToRedmoon();
                    break;
                case PublicEventPhase.Redmoon:
                    PhaseRedmoon();
                    break;
            }
        }

        private void PhaseEnter()
        {
            Random rnd = new();
            uint[] Object = [0, 1];
            int Index1 = rnd.Next(Object.Length);
            if (Index1 > 0) publicEvent.ActivateObjective(PublicEventObjective.FreeCapturedLopp);
        }

        private void PhaseRandomPath()
        {
            Random rnd = new();
            uint[] Path = [0, 1];
            int Index = rnd.Next(Path.Length);
            if (Index > 0) publicEvent.SetPhase(PublicEventPhase.RandomPathFindChief);
            else publicEvent.SetPhase(PublicEventPhase.RandomPathChasm);
        }

        private void PhaseRandomPathFindChief()
        {
            publicEvent.ActivateObjective(PublicEventObjective.FindChiefKaskalak, mapInstance.PlayerCount);
            publicEvent.ActivateObjective(PublicEventObjective.MineGoldInfusedLavaCores);

            IDoorEntity door = mapInstance.GetEntity<IDoorEntity>(CaveBranchDoorGuid);
            door?.OpenDoor();

            var triggerEntity = publicEvent.CreateEntity<IWorldLocationVolumeGridTriggerEntity>();
            triggerEntity.Initialise(18273, 362);
            triggerEntity.AddToMap(mapInstance, new Vector3(-41.99735f, -904.2222f, -460.5423f));

            foreach (IPlayer player in mapInstance.GetPlayers())//TODO: send message based on character's faction
            {
                SendCommunicatorMessage(player, CommunicatorMessage.DorianWalker2);
                SendCommunicatorMessage(player, CommunicatorMessage.ArtemisZin2);
            }
        }

        private void PhaseRandomPathCave()
        {
            publicEvent.ActivateObjective(PublicEventObjective.EscortChiefKaskalak);
            publicEvent.ActivateObjective(PublicEventObjective.SurviveMoltenCavernHeat);

            IDoorEntity door = mapInstance.GetEntity<IDoorEntity>(CaveDoorGuid);
            door?.OpenDoor();
        }

        private void PhaseRandomPathChasm()
        {
            publicEvent.ActivateObjective(PublicEventObjective.CrossTheLavaFilledChasm, mapInstance.PlayerCount);
            publicEvent.ActivateObjective(PublicEventObjective.GatherPrimalFireEssences);
            publicEvent.ActivateObjective(PublicEventObjective.DontGetStruckByLaveka);

            var triggerEntity = publicEvent.CreateEntity<ITurnstileGridTriggerEntity>();
            triggerEntity.Initialise(2821, 15f, 2821);
            triggerEntity.AddToMap(mapInstance, new Vector3(-172.02f, -830.6128f, -866.1651f)); // get real trigger range and correct coordinates

            IDoorEntity door = mapInstance.GetEntity<IDoorEntity>(ChasmBranchDoorGuid);
            door?.OpenDoor();

            foreach (IPlayer player in mapInstance.GetPlayers())
            {
                SendCommunicatorMessage(player, CommunicatorMessage.DorianWalker3);
                SendCommunicatorMessage(player, CommunicatorMessage.ArtemisZin3);
            }
        }

        private void PhaseBosun()
        {
            publicEvent.ResetObjective(PublicEventObjective.SurviveMoltenCavernHeat);
            publicEvent.ResetObjective(PublicEventObjective.DontGetStruckByLaveka);
            publicEvent.ResetObjective(PublicEventObjective.CrossTheLavaFilledChasm);
            publicEvent.ResetObjective(PublicEventObjective.EscortChiefKaskalak);
            publicEvent.ResetObjective(PublicEventObjective.FindAWayAcrossTheLava);
            publicEvent.ResetObjective(PublicEventObjective.GatherPrimalFireEssences);
            publicEvent.ResetObjective(PublicEventObjective.MineGoldInfusedLavaCores);
            publicEvent.ResetObjective(PublicEventObjective.FreeCapturedLopp);

            publicEvent.ActivateObjective(PublicEventObjective.DefeatBosunOctog);

            foreach (IPlayer player in mapInstance.GetPlayers())
            {
                SendCommunicatorMessage(player, CommunicatorMessage.DorianWalker4);
                SendCommunicatorMessage(player, CommunicatorMessage.ArtemisZin4);
            }

            Random rnd = new();
            uint[] Object = [0, 1];
            int Index1 = rnd.Next(Object.Length);
            if (Index1 > 0)
            {
                publicEvent.ActivateObjective(PublicEventObjective.FreeTheRedmoonPrisoners);

                foreach (IPlayer player in mapInstance.GetPlayers())
                {
                    SendCommunicatorMessage(player, CommunicatorMessage.DorianWalker5);
                    SendCommunicatorMessage(player, CommunicatorMessage.ArtemisZin5);
                }

                int Index2 = rnd.Next(Object.Length);
                if (Index2 > 0) publicEvent.ActivateObjective(PublicEventObjective.RidSkullcanoOfMarauders);
            }
            else
            {
                publicEvent.ActivateObjective(PublicEventObjective.RidSkullcanoOfMarauders);//TODO: look on youtube if we always have an objective, or we can only have Bosun Octog
            }
        }

        private void PhasePlatform()
        {
            publicEvent.ActivateObjective(PublicEventObjective.GatherOnThePlatform, mapInstance.PlayerCount);

            var triggerEntity = publicEvent.CreateEntity<IWorldLocationVolumeGridTriggerEntity>();
            triggerEntity.Initialise(18879, 2909);
            triggerEntity.AddToMap(mapInstance, new Vector3(-712.017f, -879.7111f, -1000.969f));
            //TODO: find spline and move the players to next area
        }

        private void PhaseGetToRedmoon()
        {
            publicEvent.ResetObjective(PublicEventObjective.FreeTheRedmoonPrisoners);
            publicEvent.ActivateObjective(PublicEventObjective.KillGruharAndTakeStash);
            publicEvent.ActivateObjective(PublicEventObjective.ReachTheEldanTerraformer);

            Random rnd = new();
            uint[] Object = [0, 1];
            int Index1 = rnd.Next(Object.Length);
            if (Index1 > 0)
            {
                publicEvent.ActivateObjective(PublicEventObjective.HackMissileConsoles);

                foreach (IPlayer player in mapInstance.GetPlayers())
                {
                    SendCommunicatorMessage(player, CommunicatorMessage.DorianWalker9);
                    SendCommunicatorMessage(player, CommunicatorMessage.ArtemisZin9);
                }
            }

            var triggerEntity1 = publicEvent.CreateEntity<ITurnstileGridTriggerEntity>();
            triggerEntity1.Initialise(333, 10f, 333);                                              // Need to look into this more
            triggerEntity1.AddToMap(mapInstance, new Vector3(-1133.032f, -693.5589f, -350.6335f)); // get real trigger range and correct coordinates

            var triggerEntity2 = publicEvent.CreateEntity<ITurnstileGridTriggerEntity>();
            triggerEntity2.Initialise(8218, 5f, 8218);                                            // Need to look into this more
            triggerEntity2.AddToMap(mapInstance, new Vector3(-852.301f, -693.5006f, -460.2653f)); // get real trigger range and correct coordinates
        }

        private void PhaseRedmoon()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatMordechaiRedmoon);

            //TODO: Redmoon and Laveka talking
        }

        private void SendCommunicatorMessage(IPlayer player, CommunicatorMessage message)
        {
            ICommunicatorMessage communicatorMessage = globalQuestManager.GetCommunicatorMessage(message);
            communicatorMessage?.Send(player.Session);
        }

        public void OnPublicEventObjectiveStatus(IPublicEventObjective objective)
        {
            if (objective.Status != PublicEventStatus.Succeeded)
                return;

            switch ((PublicEventObjective)objective.Entry.Id)
            {
                case PublicEventObjective.DefeatThunderfoot:
                    publicEvent.SetPhase(PublicEventPhase.RandomPathChasm); //Remove once we fix the cave
                    break;
                /* case PublicEventObjective.DefeatThunderfoot:
                     publicEvent.SetPhase(PublicEventPhase.RandomPath); //Add back in once we fix the cave
                     break;*/
                case PublicEventObjective.FreeCapturedLopp:
                    foreach (IPlayer player in mapInstance.GetPlayers())
                    {
                        SendCommunicatorMessage(player, CommunicatorMessage.ChiefKaskalak2);
                    }
                    break;
                case PublicEventObjective.FindChiefKaskalak:
                    publicEvent.ActivateObjective(PublicEventObjective.SpeakToChiefKaskalak);
                    break;
                case PublicEventObjective.SpeakToChiefKaskalak:
                    publicEvent.SetPhase(PublicEventPhase.RandomPathCave);
                    break;
                case PublicEventObjective.EscortChiefKaskalak:
                    publicEvent.SetPhase(PublicEventPhase.Bosun);
                    break;
                case PublicEventObjective.CrossTheLavaFilledChasm:
                    publicEvent.SetPhase(PublicEventPhase.Bosun);
                    break;
                case PublicEventObjective.DefeatBosunOctog:
                    publicEvent.SetPhase(PublicEventPhase.Platform);
                    break;
                case PublicEventObjective.GatherOnThePlatform:
                    publicEvent.SetPhase(PublicEventPhase.GetToRedmoon);
                    break;
                case PublicEventObjective.KillGruharAndTakeStash:
                    publicEvent.ActivateObjective(PublicEventObjective.RedmoonTerraformer);//does it only get triggered from this miniboss?
                    break;
                case PublicEventObjective.ReachTheEldanTerraformer:
                    publicEvent.SetPhase(PublicEventPhase.Redmoon);
                    break;
                case PublicEventObjective.DefeatMordechaiRedmoon:
                    publicEvent.Finish(PublicEventTeam.PublicTeam);
                    break;
            }
        }
    }
}
