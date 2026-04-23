using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Trigger;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using System.Numerics;

namespace NexusForever.Script.Instance.Dungeon.RuinsOfKelVoreth
{
    [ScriptFilterOwnerId(161)]
    public class RuinsOfKelVorethEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;
        private IMapInstance mapInstance;

        private uint GrondGateGuid;
        private uint BloodPitGateGuid;
        private uint EldanFloorGuid;

        private enum Creature
        {
            NormalGrondTheCorpsemaker = 32534
        }

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;
        private readonly IGlobalQuestManager globalQuestManager;

        public RuinsOfKelVorethEventScript(
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
            mapInstance = publicEvent.Map as IMapInstance;

            publicEvent.SetPhase(PublicEventPhase.FightInBloodPit);
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
                case PublicEventCreature.GrondGate:
                    GrondGateGuid = worldEntity.Guid;
                    break;
                case PublicEventCreature.BloodPitGate:
                    BloodPitGateGuid = worldEntity.Guid;
                    break;
                case PublicEventCreature.DrokkDoor:
                    EldanFloorGuid = worldEntity.Guid;
                    break;
            }
        }
        public void OnPublicEventPhase(uint phase)
        {
            switch ((PublicEventPhase)phase)
            {
                case PublicEventPhase.FightInBloodPit:
                    OnPhaseFightInBloodPit();
                    break;
                case PublicEventPhase.GrondTheCorpsemaker:
                    OnPhaseGrondTheCorpsemaker();
                    break;
                case PublicEventPhase.SlaveMasterDrokk:
                    OnPhaseSlaveMasterDrokk();
                    break;
                case PublicEventPhase.ForgeMasterTrogun:
                    OnPhaseForgeMasterTrogun();
                    break;
            }
        }

        private void OnPhaseFightInBloodPit()
        {
            publicEvent.ActivateObjective(PublicEventObjective.FightYourWayThroughTheBloodPit, 3);

            var triggerEntity = publicEvent.CreateEntity<ITurnstileGridTriggerEntity>();
            triggerEntity.Initialise(445, 10f, 445);// get real range
            triggerEntity.AddToMap(mapInstance, new Vector3(108.5f, -881.2f, 133.2f));// get real coordinates

            //TODO: Look if communicator activates once you load the map, or once you or your group take a step
            //Add blood pit announcer
        }

        private void OnPhaseGrondTheCorpsemaker()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatGrondTheCorpsemaker);

            IDoorEntity door = mapInstance.GetEntity<IDoorEntity>(GrondGateGuid);
            door?.OpenDoor();

            //TODO: Make all other mobs run away
            //Spawn boss
        }

        private void OnPhaseSlaveMasterDrokk()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatSlavemasterDrokk);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatDarkwitchGurka);
            publicEvent.ActivateObjective(PublicEventObjective.DodgingTheDefense);

            Random rnd = new();
            uint[] Object = [0, 1];
            int Index1 = rnd.Next(Object.Length);
            if (Index1 > 0) publicEvent.ActivateObjective(PublicEventObjective.PutTheKelVorethSlavesOutOfTheirMisery);

            int Index2 = rnd.Next(Object.Length);
            if (Index2 > 0) publicEvent.ActivateObjective(PublicEventObjective.AccessTheHiddenEldanDataStorageDevices);// before cross faction it looks random, but always here after that, but then again its only 5 vidoes, could be still random

            IDoorEntity door = mapInstance.GetEntity<IDoorEntity>(BloodPitGateGuid);
            door?.OpenDoor();

            var triggerEntity1 = publicEvent.CreateEntity<ITurnstileGridTriggerEntity>();
            triggerEntity1.Initialise(847, 100f, 847);// get real range
            triggerEntity1.AddToMap(mapInstance, new Vector3(610.2f, -877.1f, 522.3f));// get real coordinates
        }

        private void OnPhaseForgeMasterTrogun()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatForgemasterTrogun);

            Random rnd = new();
            uint[] Object = [0, 1];
            int Index1 = rnd.Next(Object.Length);
            if (Index1 > 0) publicEvent.ActivateObjective(PublicEventObjective.DestroyKelVorethForges);

            int Index2 = rnd.Next(Object.Length);
            if (Index2 > 0) publicEvent.ActivateObjective(PublicEventObjective.KillBattleswornAndDarkwitchOsun);

            int Index3 = rnd.Next(Object.Length);
            if (Index3 > 0) publicEvent.ActivateObjective(PublicEventObjective.BurnKelVorethWarSupplies);

            IDoorEntity door = mapInstance.GetEntity<IDoorEntity>(EldanFloorGuid);
            door?.OpenDoor();
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
                case PublicEventObjective.FightYourWayThroughTheBloodPit:
                    publicEvent.SetPhase(PublicEventPhase.GrondTheCorpsemaker);
                    break;
                case PublicEventObjective.DefeatGrondTheCorpsemaker:
                    publicEvent.SetPhase(PublicEventPhase.SlaveMasterDrokk);
                    break;
                case PublicEventObjective.DefeatSlavemasterDrokk:
                    publicEvent.SetPhase(PublicEventPhase.ForgeMasterTrogun);
                    break;
                case PublicEventObjective.DefeatForgemasterTrogun:
                    publicEvent.Finish(PublicEventTeam.PublicTeam);
                    break;
            }
        }
    }
}
