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

namespace NexusForever.Script.Instance.Expedition.Gauntlet
{
    [ScriptFilterOwnerId(446)]
    public class GauntletEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;
        private IMapInstance mapInstance;

        private uint airLockDoorGuid;

        private uint gatherRingTriggerGuid;

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;
        private readonly IGlobalQuestManager globalQuestManager;

        public GauntletEventScript(
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

            publicEvent.SetPhase(PublicEventPhase.TalkToPilotTaboro);
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

            }
        }

        private void OnAddToMapWorldLocationEntity(IWorldLocationVolumeGridTriggerEntity worldLocationEntity)
        {
            switch (worldLocationEntity.Entry.Id)
            {
                case 38909:
                     gatherRingTriggerGuid = worldLocationEntity.Guid;
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
                case PublicEventPhase.GetIntoAirlock:
                    OnPhaseGetIntoAirlock();
                    break;
                case PublicEventPhase.WhatHappened:
                    OnPhaseWhatHappened();
                    break;
                case PublicEventPhase.ActivateSwitches:
                    OnPhaseActivateSwitches();
                    break;
                case PublicEventPhase.GatherAtTheSwarmPit:
                    OnPhaseGatherAtTheSwarmPit();
                    break;
                case PublicEventPhase.SurviveTheFirstArena:
                    OnPhaseSurviveTheFirstArena();
                    break;
                case PublicEventPhase.EnterTheChamberOfChoices:
                    OnPhaseEnterTheChamberOfChoices();
                    break;
                case PublicEventPhase.SurviveTheSplorg:
                    OnPhaseSurviveTheSplorg();
                    break;
                case PublicEventPhase.ChamberOfChoices:
                    OnPhaseChamberOfChoices();
                    break;
                case PublicEventPhase.EnterTheMainEventArena:
                    OnPhaseEnterTheMainEventArena();
                    break;
                case PublicEventPhase.SurviveTheMainEvent:
                    OnPhaseSurviveTheMainEvent();
                    break;
                case PublicEventPhase.DefeatSliceAndDice:
                    OnPhaseDefeatSliceAndDice();
                    break;
                case PublicEventPhase.DefeatTheShockKing:
                    OnPhaseDefeatTheShockKing();
                    break;
                case PublicEventPhase.DefeatBrickBraggor:
                    OnPhaseDefeatBrickBraggor();
                    break;
                case PublicEventPhase.TalkToNPC:
                    OnPhaseTalkToNPC();
                    break;
            }
        }

        private void OnPhaseGetIntoAirlock()
        {
            publicEvent.ActivateObjective(PublicEventObjective.GetIntoAirlock, mapInstance.PlayerCount);

            IDoorEntity door = mapInstance.GetEntity<IDoorEntity>(airLockDoorGuid);
            door?.OpenDoor();

            var triggerEntity = publicEvent.CreateEntity<IWorldLocationVolumeGridTriggerEntity>();
            triggerEntity.Initialise(38909, 5735);
            triggerEntity.AddToMap(mapInstance, new Vector3(523.0805f, 0.1994047f, -507.9437f));
        }

        private void OnPhaseWhatHappened()
        {
            publicEvent.ActivateObjective(PublicEventObjective.FindOutWhatHappenedToYou);

             foreach (IPlayer player in mapInstance.GetPlayers())
                player.CinematicManager.QueueCinematic(cinematicFactory.CreateCinematic<IGauntletFindOutWhatHappened>());

            foreach (IPlayer player in mapInstance.GetPlayers())// this needs to happen after cinematic is finished
                player.TeleportToLocal(new Vector3(-1006.839f, 2.617371f, 1187.995f));// looks like every player teleport teleported to a new coordinate, 38911, 38912, 38913, 38914
        }

        private void OnPhaseActivateSwitches()
        {
            publicEvent.ActivateObjective(PublicEventObjective.ActivateSwitches);
            publicEvent.ActivateObjective(PublicEventObjective.CollectGoldenSkulls);
        }

        private void OnPhaseGatherAtTheSwarmPit()
        {
            publicEvent.ActivateObjective(PublicEventObjective.GatherAtTheSwarmPit, mapInstance.PlayerCount);

            var triggerEntity = publicEvent.CreateEntity<IWorldLocationVolumeGridTriggerEntity>();
            triggerEntity.Initialise(39001, 5753);
            triggerEntity.AddToMap(mapInstance, new Vector3(-1007.078f, 5.185604E-06f, 1063.784f));
        }

        private void OnPhaseSurviveTheFirstArena()
        {
            publicEvent.ActivateObjective(PublicEventObjective.SurviveTheFirstArena);
        }

        private void OnPhaseEnterTheChamberOfChoices()
        {
            publicEvent.ActivateObjective(PublicEventObjective.EnterTheChamberOfChoices, mapInstance.PlayerCount);
        }

        private void OnPhaseSurviveTheSplorg()
        {
            publicEvent.ActivateObjective(PublicEventObjective.SurviveTheSplorg);
            publicEvent.ActivateObjective(PublicEventObjective.CollectGauntletScoreTokens);// I am not sure this is the correct one, also this is added with a delay and not with SurviveTheSplorg objective, like 10 seconds later
        }

        private void OnPhaseChamberOfChoices()
        {
            publicEvent.ActivateObjective(PublicEventObjective.EnterTheFactionFrictionArena, mapInstance.PlayerCount);
            publicEvent.ActivateObjective(PublicEventObjective.EnterTheCharnelChamber, mapInstance.PlayerCount);
            publicEvent.ActivateObjective(PublicEventObjective.EndorseAProduct);
            publicEvent.ActivateObjective(PublicEventObjective.SaveGauntletContestantsFromTheDarkspur);
        }

        private void OnPhaseEnterTheMainEventArena()
        {
            publicEvent.ActivateObjective(PublicEventObjective.EnterTheMainEventArena, mapInstance.PlayerCount);
        }

        private void OnPhaseSurviveTheMainEvent()
        {
            publicEvent.ActivateObjective(PublicEventObjective.SurviveTheMainEvent);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheGoonSquad);// this gets added with a delay
        }

        private void OnPhaseDefeatSliceAndDice()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatSliceAndDice);
        }

        private void OnPhaseDefeatTheShockKing()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheShockKing);
        }

        private void OnPhaseDefeatBrickBraggor()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatBrickBraggor);

            foreach (IPlayer player in mapInstance.GetPlayers())
                player.CinematicManager.QueueCinematic(cinematicFactory.CreateCinematic<IGauntletOnSpawnBrickBraggor>());// make real one
        }

        private void OnPhaseTalkToNPC()
        {
            publicEvent.ActivateObjective(PublicEventObjective.TalkToJudgeKain);// we need to check players faction and give them the correct objective
            publicEvent.ActivateObjective(PublicEventObjective.TalkToAgentLex);
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
                case PublicEventObjective.TalkToPilotTaboro:
                    publicEvent.SetPhase(PublicEventPhase.GetIntoAirlock);
                    break;
                case PublicEventObjective.GetIntoAirlock:
                    publicEvent.SetPhase(PublicEventPhase.WhatHappened);
                    break;
                case PublicEventObjective.FindOutWhatHappenedToYou:
                    publicEvent.SetPhase(PublicEventPhase.ActivateSwitches);
                    break;
                case PublicEventObjective.ActivateSwitches:
                    publicEvent.SetPhase(PublicEventPhase.GatherAtTheSwarmPit);
                    break;
                case PublicEventObjective.GatherAtTheSwarmPit:
                    publicEvent.SetPhase(PublicEventPhase.SurviveTheFirstArena);
                    break;
                case PublicEventObjective.SurviveTheFirstArena:
                    publicEvent.SetPhase(PublicEventPhase.EnterTheChamberOfChoices);
                    break;
                case PublicEventObjective.EnterTheChamberOfChoices:
                    publicEvent.SetPhase(PublicEventPhase.SurviveTheSplorg);
                    break;
                case PublicEventObjective.SurviveTheSplorg:
                    publicEvent.SetPhase(PublicEventPhase.ChamberOfChoices);
                    break;
                case PublicEventObjective.EnterTheCharnelChamber:
                    publicEvent.ActivateObjective(PublicEventObjective.KillTheChampionator);
                    break;
                case PublicEventObjective.EnterTheFactionFrictionArena:
                    publicEvent.ActivateObjective(PublicEventObjective.KillTheOpposingFactionsTeam); // this will need some more work, it adds the object with a delay, and I am not sure what spawns for domie run or cross faction run
                    break;
                case PublicEventObjective.KillTheOpposingFactionsTeam:
                case PublicEventObjective.KillTheChampionator:
                    publicEvent.SetPhase(PublicEventPhase.EnterTheMainEventArena);
                    break;
                case PublicEventObjective.EnterTheMainEventArena:
                    publicEvent.SetPhase(PublicEventPhase.SurviveTheMainEvent);
                    break;
                case PublicEventObjective.DefeatTheGoonSquad:
                    publicEvent.SetPhase(PublicEventPhase.DefeatSliceAndDice);
                    break;
                case PublicEventObjective.DefeatSliceAndDice:
                    publicEvent.SetPhase(PublicEventPhase.DefeatTheShockKing);
                    break;
                case PublicEventObjective.DefeatTheShockKing:
                    publicEvent.SetPhase(PublicEventPhase.DefeatBrickBraggor);
                    break;
                case PublicEventObjective.DefeatBrickBraggor:
                    publicEvent.SetPhase(PublicEventPhase.TalkToNPC);
                    break;
                case PublicEventObjective.TalkToJudgeKain:
                    publicEvent.Finish(PublicEventTeam.PublicTeam);
                    break;
                case PublicEventObjective.TalkToAgentLex:
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
                case PublicEventPhase.WhatHappened:
                    foreach (IPlayer player1 in mapInstance.GetPlayers())
                    player1.TeleportToLocal(new Vector3(-1006.839f, 2.617371f, 1187.995f)); // looks like every player teleport teleported to a new coordinate, 38911, 38912, 38913, 38914
                    break;
            }
        }
    }
}
