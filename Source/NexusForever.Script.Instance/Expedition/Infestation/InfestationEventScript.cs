using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Trigger;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Expedition.Infestation
{
    [ScriptFilterOwnerId(95)]
    public class InfestationEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;
        private IMapInstance mapInstance;

        private uint doorOneGuid;
        private uint openVentGuid;

        #region Dependency Injection

        private readonly IGlobalQuestManager globalQuestManager;

        public InfestationEventScript(
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

            publicEvent.SetPhase(PublicEventPhase.ProceedOntoTheCargoShip);
            //publicEvent.SetPhase(PublicEventPhase.CloseTheShipVents);
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

        private void OnAddToMapWorldLocationEntity(IWorldLocationVolumeGridTriggerEntity worldLocationEntity)
        {
            switch (worldLocationEntity.Entry.Id)
            {

            }
        }

        private void OnAddToMapWorldEntity(IWorldEntity worldEntity)
        {
            switch ((PublicEventCreature)worldEntity.CreatureId)
            {
                case PublicEventCreature.OpenVent:
                    openVentGuid = worldEntity.Guid;
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

        /// <summary>
        /// Invoked when the public event phase changes.
        /// </summary>
         public void OnPublicEventPhase(uint phase)
         {
             switch ((PublicEventPhase)phase)
             {
                case PublicEventPhase.ProceedOntoTheCargoShip:
                    PhaseProceedPntoTheCargoShip();
                    break;
                case PublicEventPhase.CloseTheShipVents:
                    PhaseCloseTheShipVents();
                    break;
                case PublicEventPhase.FindTheMedicalBay:
                    PhaseFindTheMedicalBay();
                    break;
                case PublicEventPhase.SealHullBreaches:
                    PhaseSealHullBreaches();
                    break;
                case PublicEventPhase.FindTheMedicalBay2:
                    PhaseFindTheMedicalBay2();
                    break;
                case PublicEventPhase.FindMedicalSupplies:
                    PhaseFindMedicalSupplies();
                    break;
                case PublicEventPhase.DefeatTheAttackOnMedbay:
                    PhaseDefeatTheAttackOnMedbay();
                    break;
                case PublicEventPhase.FindMedicalSupplies2:
                    PhaseFindMedicalSupplies2();
                    break;
             }
         }

        private void PhaseProceedPntoTheCargoShip()
        {
            publicEvent.ActivateObjective(PublicEventObjective.ProceedOntoTheCargoShip, mapInstance.PlayerCount);

            var triggerEntity = publicEvent.CreateEntity<ITurnstileGridTriggerEntity>();
            triggerEntity.Initialise(1005, 50f, 1005);// get real distance
            triggerEntity.AddToMap(mapInstance, new Vector3(-0.2391071f, -499.99f, 87.62592f));// get real coordinates
        }

        private void PhaseCloseTheShipVents()
        {
            publicEvent.ActivateObjective(PublicEventObjective.CloseTheShipVents);

        }

        private void PhaseFindTheMedicalBay()
        {
            IDoorEntity door = mapInstance.GetEntity<IDoorEntity>(doorOneGuid);
            door?.OpenDoor();

            publicEvent.ActivateObjective(PublicEventObjective.FindTheMedicalBay, mapInstance.PlayerCount);
            publicEvent.ActivateObjective(PublicEventObjective.TagValuableCargo);
        }

        private void PhaseSealHullBreaches()
        {
            publicEvent.ResetObjective(PublicEventObjective.FindTheMedicalBay);
            publicEvent.ActivateObjective(PublicEventObjective.SealHullBreaches);
        }

        private void PhaseFindTheMedicalBay2()
        {
            publicEvent.ActivateObjective(PublicEventObjective.FindTheMedicalBay);
        }

        private void PhaseFindMedicalSupplies()
        {
            publicEvent.ActivateObjective(PublicEventObjective.FindMedicalSupplies);
        }

        private void PhaseDefeatTheAttackOnMedbay()
        {
            publicEvent.ResetObjective(PublicEventObjective.FindMedicalSupplies);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheAttackOnMedbay);
        }

        private void PhaseFindMedicalSupplies2()
        {
            publicEvent.ActivateObjective(PublicEventObjective.FindMedicalSupplies);
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
                case PublicEventObjective.ProceedOntoTheCargoShip:
                    publicEvent.SetPhase(PublicEventPhase.CloseTheShipVents);
                    break;
                case PublicEventObjective.CloseTheShipVents:
                    publicEvent.SetPhase(PublicEventPhase.FindTheMedicalBay);
                    break;
                case PublicEventObjective.SealHullBreaches:
                    publicEvent.SetPhase(PublicEventPhase.FindTheMedicalBay2);
                    break;
                case PublicEventObjective.DefeatTheAttackOnMedbay:
                    publicEvent.SetPhase(PublicEventPhase.FindMedicalSupplies2);
                    break;
                case PublicEventObjective.FindMedicalSupplies:
                    publicEvent.SetPhase(PublicEventPhase.HealContaminatedShiphands);
                    break;
                case PublicEventObjective.KillCyclopeanParasite:
                case PublicEventObjective.KillLumberingParasites:
                    publicEvent.Finish(PublicEventTeam.PublicTeam);
                    break;
            }
        }
    }
}
