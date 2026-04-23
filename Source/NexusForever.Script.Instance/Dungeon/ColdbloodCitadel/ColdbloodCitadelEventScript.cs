using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Entity.Trigger;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Dungeon.ColdbloodCitadel
{
    [ScriptFilterOwnerId(907)]
    public class ColdbloodCitadelEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;
        private IMapInstance mapInstance;

        private uint gatherRingGuid;
        private uint gatherRingTriggerGuid;

        private uint IceDoorGuid;

        private enum Creature
        {
            HarizogColdbloodBoss = 75459
        }


        #region Dependency Injection

        private readonly IGlobalQuestManager globalQuestManager;
        private readonly IEntitySummonFactory entitySummonFactory;
        private readonly ICreatureInfoManager creatureInfoManager;

        public ColdbloodCitadelEventScript(
            IGlobalQuestManager globalQuestManager,
            IEntitySummonFactory entitySummonFactory,
            ICreatureInfoManager creatureInfoManager)
        {
            this.globalQuestManager = globalQuestManager;
            this.entitySummonFactory = entitySummonFactory;
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion


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
                case PublicEventCreature.GatherRing:
                    gatherRingGuid = worldEntity.Guid;
                    break;
            }
        }

        private void OnAddToMapWorldLocationEntity(IWorldLocationVolumeGridTriggerEntity worldLocationEntity)
        {
            switch (worldLocationEntity.Entry.Id)
            {
                case 75624:
                    gatherRingTriggerGuid = worldLocationEntity.Guid;
                    break;
            }
        }

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
                case PublicEventCreature.GatherRing:
                    gatherRingGuid = 0;
                    break;
            }
        }

        private void OnRemoveFromMapWorldLocationEntity(IWorldLocationVolumeGridTriggerEntity worldLocationEntity)
        {
            switch (worldLocationEntity.Entry.Id)
            {
                case 75624:
                    gatherRingTriggerGuid = 0;
                    break;
            }
        }

        public void OnPublicEventPhase(uint phase)
        {
            switch ((PublicEventPhase)phase)
            {
                case PublicEventPhase.Enter:
                    OnPhaseEnter();
                    break;
                case PublicEventPhase.HailStoneGatecrasher:
                    OnPhaseHailStoneGatecrasher();
                    break;
                case PublicEventPhase.IceBloodCover:
                    OnPhaseIceBloodCover();
                    break;
                case PublicEventPhase.RiserHarizog:
                    OnPhaseRiserHarizog();
                    break;
            }
        }

        private void OnPhaseEnter()
        {
            var triggerEntity = publicEvent.CreateEntity<IWorldLocationVolumeGridTriggerEntity>();
            triggerEntity.Initialise(53206, 8656);
            triggerEntity.AddToMap(mapInstance, new Vector3(604.33f, -475.452f, -322.957f));
            //find a way to count players
        }

        private void OnPhaseHailStoneGatecrasher()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatHailStoneGatecrasher);

            var triggerEntity = mapInstance.GetEntity<IWorldLocationVolumeGridTriggerEntity>(gatherRingTriggerGuid);
            triggerEntity?.RemoveFromMap();

            var gatherRing = mapInstance.GetEntity<IWorldEntity>(gatherRingGuid);
            gatherRing?.RemoveFromMap();

            publicEvent.ActivateObjective(PublicEventObjective.SavePellFightingTheOsun);

            Random rnd = new();
            uint[] Object = [0, 1];
            int Index1 = rnd.Next(Object.Length);
            if (Index1 == 1) publicEvent.ActivateObjective(PublicEventObjective.StealSampleOfLiquidSoulfrost);

            int Index2 = rnd.Next(Object.Length);
            if (Index2 == 1) publicEvent.ActivateObjective(PublicEventObjective.GatherSoulfrostShards);

            int Index3 = rnd.Next(Object.Length);
            if (Index3 > 0) publicEvent.ActivateObjective(PublicEventObjective.RallyTheWinterfuryPell);

            BroadcastCommunicatorMessage(CommunicatorMessage.TowerEngineerRenhakul1);
        }

        private void OnPhaseIceBloodCover()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheIcebloodCoven);
            publicEvent.ActivateObjective(PublicEventObjective.RescueThePellArchitect);
            publicEvent.ActivateObjective(PublicEventObjective.KillKrovakSummonersAndTheirFrostguards);
            publicEvent.ActivateObjective(PublicEventObjective.ConcurrentCovenCollapse);

            BroadcastCommunicatorMessage(CommunicatorMessage.TowerEngineerRenhakul11);

            IDoorEntity door = mapInstance.GetEntity<IDoorEntity>(IceDoorGuid);
            door?.OpenDoor();

            Random rnd = new();
            uint[] Object = [0, 1];
            int Index1 = rnd.Next(Object.Length);
            if (Index1 == 1) publicEvent.ActivateObjective(PublicEventObjective.RescueWinterfuryPrisoners);

            int Index2 = rnd.Next(Object.Length);
            if (Index2 > 0) publicEvent.ActivateObjective(PublicEventObjective.DestroySoulrotCanisters);

            int Index3 = rnd.Next(Object.Length);
            if (Index3 > 0) publicEvent.ActivateObjective(PublicEventObjective.DisableSoulfrostTraps);// I never seen this objective on youtube, and I have no idea what phase it's from, so we may need to edit this later
        }

        private void OnPhaseRiserHarizog()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheRisenHarizog);
            publicEvent.ActivateObjective(PublicEventObjective.InfusionInterdiction);

            BroadcastCommunicatorMessage(CommunicatorMessage.TowerEngineerRenhakul4);
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
                case PublicEventObjective.FindThePellAttackingColdbloodCitadel:
                    publicEvent.SetPhase(PublicEventPhase.HailStoneGatecrasher);
                    break;
                case PublicEventObjective.DefeatHailStoneGatecrasher:
                    publicEvent.SetPhase(PublicEventPhase.IceBloodCover);
                    break;
                case PublicEventObjective.DefeatTheIcebloodCoven:
                    publicEvent.SetPhase(PublicEventPhase.RiserHarizog);
                    break;
                case PublicEventObjective.DefeatTheRisenHarizog:
                    publicEvent.Finish(PublicEventTeam.PublicTeam);
                    break;
            }
        }
    }
}
