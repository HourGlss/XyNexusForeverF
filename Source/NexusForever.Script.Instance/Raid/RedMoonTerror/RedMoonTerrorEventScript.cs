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

namespace NexusForever.Script.Instance.Raid.RedMoonTerror
{
    [ScriptFilterOwnerId(705)]
    public class RedMoonTerrorEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;
        private IMapInstance mapInstance;


        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;
        private readonly IGlobalQuestManager globalQuestManager;

        public RedMoonTerrorEventScript(
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
                case PublicEventPhase.ChiefWardenLockjaw:
                    PhaseChiefWardenLockjaw();
                    break;
                case PublicEventPhase.InvestigateTheShredder:
                    PhaseInvestigateTheShredder();
                    break;
                case PublicEventPhase.DefeatSwabbieSkiLi:
                    PhaseDefeatSwabbieSkiLi();
                    break;
                case PublicEventPhase.WasteRelocationShafts:
                    PhaseWasteRelocationShafts();
                    break;
                case PublicEventPhase.Robomination:
                    PhaseRobomination();
                    break;
                case PublicEventPhase.EngineeringCompartment:
                    PhaseEngineeringCompartment();
                    break;
                case PublicEventPhase.EngiMinibosses:
                    PhaseEngiMinibosses();
                    break;
                case PublicEventPhase.TheEngineers:
                    PhaseTheEngineers();
                    break;
                case PublicEventPhase.EnterCrewQuarters:
                    PhaseEnterCrewQuarters();
                    break;
                case PublicEventPhase.MordechaiRedmoon:
                    PhaseMordechaiRedmoon();
                    break;
                case PublicEventPhase.DestroyAntiBoardingTurrets:
                    PhaseDestroyAntiBoardingTurrets();
                    break;
                case PublicEventPhase.StarEater:
                    PhaseStarEater();
                    break;
                case PublicEventPhase.BreakBackIntoTheRedmoonTerror:
                    PhaseBreakBackIntoTheRedmoonTerror();
                    break;
                case PublicEventPhase.MarauderOfficers:
                    PhaseMarauderOfficers();
                    break;
                case PublicEventPhase.FindTheNavigationCore:
                    PhaseFindTheNavigationCore();
                    break;
                case PublicEventPhase.Starmap:
                    PhaseStarmap();
                    break;
                case PublicEventPhase.Medbay:
                    PhaseMedbay();
                    break;
                case PublicEventPhase.Morgue:
                    PhaseMorgue();
                    break;
                case PublicEventPhase.Laveka:
                    PhaseLaveka();
                    break;
            }
        }

        private void PhaseEnter()
        {

        }

        private void PhaseChiefWardenLockjaw()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatChiefWardenLockjaw);
        }

        private void PhaseInvestigateTheShredder()
        {
            publicEvent.ActivateObjective(PublicEventObjective.InvestigateTheShredder);
        }

        private void PhaseDefeatSwabbieSkiLi()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatSwabbieSkiLi);
        }

        private void PhaseWasteRelocationShafts()
        {
            publicEvent.ActivateObjective(PublicEventObjective.EnterTheWasteRelocationShafts);
        }

        private void PhaseRobomination()
        {
            BroadcastCommunicatorMessage(CommunicatorMessage.IshamelTheBloodied552);

            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheRobomination);
        }

        private void PhaseEngineeringCompartment()
        {
            publicEvent.ActivateObjective(PublicEventObjective.FindAWayToTheEngineeringCompartment);
            BroadcastCommunicatorMessage(CommunicatorMessage.IshamelTheBloodied224);
        }

        private void PhaseEngiMinibosses()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatAssistantTechnicianSkooty);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatChiefEngineScrubberThrag);
        }

        private void PhaseTheEngineers()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheEngineers);
        }

        private void PhaseEnterCrewQuarters()
        {
            publicEvent.ActivateObjective(PublicEventObjective.MakeYourWayToTheCrewQuarters);
        }

        private void PhaseMordechaiRedmoon()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatMordechaiRedmoon);

            // minibosses act weird, rare minibosses "run away" if you skip them and they get replaced by common
            // ones, but looks like the common ones come back anyway
        }

        private void PhaseDestroyAntiBoardingTurrets()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DestroyTheAntiBoardingTurret);
        }

        private void PhaseStarEater()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatStarEaterTheVoracious);
        }

        private void PhaseBreakBackIntoTheRedmoonTerror()
        {
            publicEvent.ActivateObjective(PublicEventObjective.BreakBackIntoTheRedmoonTerror);
        }

        private void PhaseMarauderOfficers()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatMarauderOfficers);
        }

        private void PhaseFindTheNavigationCore()
        {
            publicEvent.ActivateObjective(PublicEventObjective.FindTheNavigationCore);
        }

        private void PhaseStarmap()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheStarmapSimulation);
        }

        private void PhaseMedbay()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatBonedoctorMuburu);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatHeadshrinkerWgasa);
        }

        private void PhaseMorgue()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTiny);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatUntombedHorror);
        }

        private void PhaseLaveka()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatLavekaTheDarkHearted);
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
                case PublicEventObjective.SurviveTheBrig:
                    publicEvent.SetPhase(PublicEventPhase.ChiefWardenLockjaw);
                    break;
                case PublicEventObjective.DefeatChiefWardenLockjaw:
                    publicEvent.SetPhase(PublicEventPhase.InvestigateTheShredder);
                    break;
                case PublicEventObjective.InvestigateTheShredder:
                    publicEvent.SetPhase(PublicEventPhase.DefeatSwabbieSkiLi);
                    break;
                case PublicEventObjective.DefeatSwabbieSkiLi:
                    publicEvent.SetPhase(PublicEventPhase.WasteRelocationShafts);
                    break;
                case PublicEventObjective.EnterTheWasteRelocationShafts:
                    publicEvent.SetPhase(PublicEventPhase.Robomination);
                    break;
                case PublicEventObjective.DefeatTheRobomination:
                    publicEvent.SetPhase(PublicEventPhase.EngineeringCompartment);
                    break;
                case PublicEventObjective.FindAWayToTheEngineeringCompartment:
                    publicEvent.SetPhase(PublicEventPhase.EngiMinibosses);
                    break;
                case PublicEventObjective.DefeatAssistantTechnicianSkooty:
                case PublicEventObjective.DefeatChiefEngineScrubberThrag:
                    publicEvent.SetPhase(PublicEventPhase.TheEngineers);
                    break;
                case PublicEventObjective.DefeatTheEngineers:
                    publicEvent.SetPhase(PublicEventPhase.EnterCrewQuarters);
                    break;
                case PublicEventObjective.MakeYourWayToTheCrewQuarters:
                    publicEvent.SetPhase(PublicEventPhase.MordechaiRedmoon);
                    break;
                case PublicEventObjective.DefeatMordechaiRedmoon:
                    publicEvent.SetPhase(PublicEventPhase.DestroyAntiBoardingTurrets);
                    break;
                case PublicEventObjective.DestroyTheAntiBoardingTurret:
                    publicEvent.SetPhase(PublicEventPhase.StarEater);
                    break;
                case PublicEventObjective.DefeatStarEaterTheVoracious:
                    publicEvent.SetPhase(PublicEventPhase.BreakBackIntoTheRedmoonTerror);
                    break;
                case PublicEventObjective.BreakBackIntoTheRedmoonTerror:
                    publicEvent.SetPhase(PublicEventPhase.MarauderOfficers);
                    break;
                case PublicEventObjective.DefeatMarauderOfficers:
                    publicEvent.SetPhase(PublicEventPhase.FindTheNavigationCore);
                    break;
                case PublicEventObjective.FindTheNavigationCore:
                    publicEvent.SetPhase(PublicEventPhase.Starmap);
                    break;
                case PublicEventObjective.DefeatTheStarmapSimulation:
                    publicEvent.SetPhase(PublicEventPhase.Medbay);
                    break;
                case PublicEventObjective.DefeatBonedoctorMuburu:
                case PublicEventObjective.DefeatHeadshrinkerWgasa:
                    publicEvent.SetPhase(PublicEventPhase.Morgue);
                    break;
                case PublicEventObjective.DefeatTiny:
                case PublicEventObjective.DefeatUntombedHorror:
                    publicEvent.SetPhase(PublicEventPhase.Laveka);
                    break;
                case PublicEventObjective.DefeatLavekaTheDarkHearted:
                    publicEvent.Finish(PublicEventTeam.PublicTeam);
                    break;// this is not how it should be, this is only temp
            }
        }

        /// <summary>
        /// Invoked when a cinematic for <see cref="IPlayer"/> has finished.
        /// </summary>
        public void OnCinematicFinish(IPlayer player, uint cinematicId)
        {
            switch ((PublicEventPhase)publicEvent.Phase)
            {

            }
        }
    }
}
