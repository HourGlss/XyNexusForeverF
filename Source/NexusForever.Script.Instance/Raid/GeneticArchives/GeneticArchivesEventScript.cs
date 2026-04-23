using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Trigger;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Event;
using NexusForever.Script.Template.Filter;
using NexusForever.Game.Abstract.Cinematic.Cinematics;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Static.PublicEvent;

namespace NexusForever.Script.Instance.Raid.GeneticArchives
{
    [ScriptFilterOwnerId(159)]
    public class GeneticArchivesEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;
        private IMapInstance mapInstance;


        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;
        private readonly IGlobalQuestManager globalQuestManager;

        public GeneticArchivesEventScript(
            ICinematicFactory cinematicFactory,
            IGlobalQuestManager globalQuestManager)
        {
            this.cinematicFactory = cinematicFactory;
            this.globalQuestManager = globalQuestManager;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IPublicEvent owner)
        {
            publicEvent = owner;
            publicEvent.SetPhase(PublicEventPhase.Enter);

            mapInstance = publicEvent.Map as IMapInstance;
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
                case PublicEventPhase.Enter:
                    PhaseEnter();
                    break;
                case PublicEventPhase.GetToNextLevel:
                    PhaseGetToNextLevel();
                    break;
                case PublicEventPhase.SecondFloor:
                    PhaseSecondFloor();
                    break;
                case PublicEventPhase.ArchiveDefenseSystem:
                    PhaseArchiveDefenseSystem();
                    break;
                case PublicEventPhase.PhagebornConvergence:
                    PhasePhagebornConvergence();
                    break;
                case PublicEventPhase.Minibosses:
                    PhaseMinibosses();
                    break;
                case PublicEventPhase.Ohmna:
                    PhaseOhmna();
                    break;
            }
        }
        private void PhaseEnter()
        {
            BroadcastCommunicatorMessage(CommunicatorMessage.TheDreadphageOhmna1);
        }

        private void PhaseGetToNextLevel()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheFetidMiscreation);
        }

        private void PhaseSecondFloor()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatPhagetechGuardianC148);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatPhagetechGuardianC432);
        }

        private void PhaseArchiveDefenseSystem()
        {
            publicEvent.ActivateObjective(PublicEventObjective.GetThroughTheArchiveDefenseSystem);
            publicEvent.ActivateObjective(PublicEventObjective.DeathFromAbove);
        }

        private void PhasePhagebornConvergence()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatThePhagebornConvergence);
        }

        private void PhaseMinibosses()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheMalfunctioningPiston);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheMalfunctioningBattery);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheMalfunctioningGear);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheMalfunctioningDynamo);
        }

        private void PhaseOhmna()
        {
            foreach (IPlayer player in mapInstance.GetPlayers())
               player.CinematicManager.QueueCinematic(cinematicFactory.CreateCinematic<IGeneticArchivesOpenOhmnaDoor>());

            publicEvent.ActivateObjective(PublicEventObjective.DefeatDreadphageOhmna);
        }

        private void SendCommunicatorMessage(IPlayer player, CommunicatorMessage message)
        {
            ICommunicatorMessage communicatorMessage = globalQuestManager.GetCommunicatorMessage(message);
            communicatorMessage?.Send(player.Session);
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
                case PublicEventObjective.DefeatExperimentX89:
                case PublicEventObjective.DefeatKuralakTheDefiler:
                    publicEvent.SetPhase(PublicEventPhase.GetToNextLevel);
                    break;
                case PublicEventObjective.DefeatTheFetidMiscreation:
                    publicEvent.SetPhase(PublicEventPhase.SecondFloor);
                    break;
                case PublicEventObjective.DefeatPhagetechGuardianC148:
                    publicEvent.ActivateObjective(PublicEventObjective.DefeatConstructsInTheCentrifuge);//this needs to be random every week, but it must be the same for everyone, so the RNG must be in some other file, so this is only temp way for doing it
                    break;
                case PublicEventObjective.DefeatConstructsInTheCentrifuge:
                    publicEvent.ActivateObjective(PublicEventObjective.DefeatPhageMaw);
                    break;
                case PublicEventObjective.DefeatPhagetechGuardianC432:
                    publicEvent.ActivateObjective(PublicEventObjective.DefeatTheParagonsOfSymbiosis);//this needs to be random every week, but it must be the same for everyone, so the RNG must be in some other file
                    break;
                case PublicEventObjective.DefeatTheParagonsOfSymbiosis:
                    publicEvent.ActivateObjective(PublicEventObjective.DefeatThePhagetechPrototypes);
                    break;
                case PublicEventObjective.DefeatPhageMaw:
                case PublicEventObjective.DefeatThePhagetechPrototypes:
                    publicEvent.SetPhase(PublicEventPhase.ArchiveDefenseSystem);
                    break;
                case PublicEventObjective.GetThroughTheArchiveDefenseSystem:
                    publicEvent.SetPhase(PublicEventPhase.PhagebornConvergence);
                    break;
                case PublicEventObjective.DefeatThePhagebornConvergence:
                    publicEvent.SetPhase(PublicEventPhase.Minibosses);
                    break;
                case PublicEventObjective.DefeatTheMalfunctioningGear:
                case PublicEventObjective.DefeatTheMalfunctioningPiston:
                case PublicEventObjective.DefeatTheMalfunctioningDynamo:
                case PublicEventObjective.DefeatTheMalfunctioningBattery:
                    publicEvent.SetPhase(PublicEventPhase.Ohmna);
                    break;
                case PublicEventObjective.DefeatDreadphageOhmna:
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
                case PublicEventPhase.Enter:
                    SendCommunicatorMessage(player, CommunicatorMessage.TheDreadphageOhmna1);//did we have a Cinematic? if not, was it a trigger sript if you walked off the spawn spot, or just spawn?
                    break;
                case PublicEventPhase.Ohmna:
                    SendCommunicatorMessage(player, CommunicatorMessage.TheDreadphageOhmna8);
                    break;
            }
        }
    }
}
