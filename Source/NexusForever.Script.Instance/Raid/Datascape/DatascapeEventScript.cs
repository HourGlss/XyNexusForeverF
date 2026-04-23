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

namespace NexusForever.Script.Instance.Raid.Datascape
{
    [ScriptFilterOwnerId(157)]
    public class DatascapeEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;
        private IMapInstance mapInstance;


        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;
        private readonly IGlobalQuestManager globalQuestManager;

        public DatascapeEventScript(
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
                case PublicEventPhase.HallsOfTheInfiniteMind:
                    PhaseHallsOfTheInfiniteMind();
                    break;
                case PublicEventPhase.TheOculus:
                    OnPhaseTheOculus();
                    break;
                case PublicEventPhase.FirstFrostBoulder:
                    PhaseFirstFrostBoulder();
                    break;
                case PublicEventPhase.SecondFrostBoulder:
                    PhaseSecondFrostBoulder();
                    break;
                case PublicEventPhase.FrostbringerWarlock:
                    PhaseFrostbringerWarlock();
                    break;
                case PublicEventPhase.MaelstromAuthority:
                    PhaseMaelstromAuthority();
                    break;
                case PublicEventPhase.AlphaElementalGuardians:
                    PhaseAlphaElementalGuardians();
                    break;
                case PublicEventPhase.AlphaPersonalityDatacore:
                    PhaseAlphaPersonalityDatacore();
                    break;
                case PublicEventPhase.EarthRoomCanimid:
                    PhaseEarthRoomCanimid();
                    break;
                case PublicEventPhase.EarthRoomRock:
                    PhaseEarthRoomRock();
                    break;
                case PublicEventPhase.Gloomclaw:
                    PhaseGloomclaw();
                    break;
                case PublicEventPhase.LogicWingRoom1:
                    PhaseLogicWingRoom1();
                    break;
                case PublicEventPhase.LogicWingRoom2:
                    PhaseLogicWingRoom2();
                    break;
                case PublicEventPhase.LogicWingRoom3:
                    PhaseLogicWingRoom3();
                    break;
                case PublicEventPhase.LogicWingLogicElemental:
                    PhaseLogicWingLogicElemental();
                    break;
                case PublicEventPhase.DeltaElementalGuardians:
                    PhaseDeltaElementalGuardians();
                    break;
                case PublicEventPhase.DeltaPersonalityDatacore:
                    PhaseDeltaPersonalityDatacore();
                    break;
                case PublicEventPhase.VolatilityLattice:
                    PhaseVolatilityLattice();
                    break;
                case PublicEventPhase.WarmongerAgratha:
                    PhaseWarmongerAgratha();
                    break;
                case PublicEventPhase.WarmongerChuna:
                    PhaseWarmongerChuna();
                    break;
                case PublicEventPhase.WarmongerTalarii:
                    PhaseWarmongerTalarii();
                    break;
                case PublicEventPhase.GrandWarmongerTargresh:
                    PhaseGrandWarmongerTargresh();
                    break;
                case PublicEventPhase.BetaElementalGuardians:
                    PhaseBetaElementalGuardians();
                    break;
                case PublicEventPhase.BetaPersonalityDatacore:
                    PhaseBetaPersonalityDatacore();
                    break;
                case PublicEventPhase.MemoryCores:
                    PhaseMemoryCores();
                    break;
                case PublicEventPhase.Avatus:
                    PhaseAvatus();
                    break;
            }
        }
        private void PhaseEnter()
        {

        }

        private void PhaseHallsOfTheInfiniteMind()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheSystemDaemons);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatOptimizedMemoryProbeED1);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatOptimizedMemoryProbeP2Z);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatOptimizedMemoryProbeTX67);
        }

        private void OnPhaseTheOculus()
        {
            BroadcastCommunicatorMessage(CommunicatorMessage.Caretaker111);
        }

        private void PhaseFirstFrostBoulder()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheFirstFrostBoulderAvalanche);

            BroadcastCommunicatorMessage(CommunicatorMessage.Caretaker112);
        }

        private void PhaseSecondFrostBoulder()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheSecondFrostBoulderAvalanche);
        }

        private void PhaseFrostbringerWarlock()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheFrostbringerWarlock);
        }

        private void PhaseMaelstromAuthority()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheMaelstromAuthority);
        }

        private void PhaseAlphaElementalGuardians()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheElementalGuardians1); //use sniffs to look what objective this is

            BroadcastCommunicatorMessage(CommunicatorMessage.Caretaker113);
        }

        private void PhaseAlphaPersonalityDatacore()
        {
            publicEvent.ActivateObjective(PublicEventObjective.RetrieveTheFirstPersonalityDatacore);//I am guessing you add the objective based on how many wings you cleared, we need to fix this later, for now I will leave like this
        }

        private void PhaseEarthRoomCanimid()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheFullyOptimizedCanimid);
        }

        private void PhaseEarthRoomRock()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheLogicGuidedRockslide);
        }

        private void PhaseGloomclaw()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatGloomclaw);
        }

        private void PhaseLogicWingRoom1()
        {
            publicEvent.ActivateObjective(PublicEventObjective.PowerUpAllOfTheEldanPowerGenerators);//look at sniffs for everything here
            publicEvent.ActivateObjective(PublicEventObjective.GeneratorCharge1);
            publicEvent.ActivateObjective(PublicEventObjective.GeneratorCharge2);
            publicEvent.ActivateObjective(PublicEventObjective.GeneratorCharge3);
        }

        private void PhaseLogicWingRoom2()
        {
            publicEvent.ActivateObjective(PublicEventObjective.PowerUpTheEldanPowerGenerators);//look at sniffs for everything here
            publicEvent.ActivateObjective(PublicEventObjective.GeneratorCharge4);
            publicEvent.ActivateObjective(PublicEventObjective.GeneratorCharge5);
            publicEvent.ActivateObjective(PublicEventObjective.GeneratorCharge6);
        }

        private void PhaseLogicWingRoom3()
        {
            publicEvent.ActivateObjective(PublicEventObjective.PowerUpAllOfTheEldanPowerGenerators3);//look at sniffs for everything here
            publicEvent.ActivateObjective(PublicEventObjective.GeneratorCharge7);
            publicEvent.ActivateObjective(PublicEventObjective.GeneratorCharge8);
            publicEvent.ActivateObjective(PublicEventObjective.GeneratorCharge9);
        }

        private void PhaseLogicWingLogicElemental()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheAbstractAugmentationAlgorithm);
        }

        private void PhaseDeltaElementalGuardians()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheElementalGuardians3);//use sniffs to get real objective
        }

        private void PhaseDeltaPersonalityDatacore()
        {
            publicEvent.ActivateObjective(PublicEventObjective.RetrieveTheSecondPersonalityDatacore);//I am guessing you add the objective based on how many wings you cleared, we need to fix this later, for now I will leave like this
        }

        private void PhaseVolatilityLattice()
        {
            publicEvent.ActivateObjective(PublicEventObjective.EscapeAvatusAttention);
            publicEvent.ActivateObjective(PublicEventObjective.TimedOut);
        }

        private void PhaseWarmongerAgratha()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatWarmongerAgratha);
        }

        private void PhaseWarmongerChuna()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatWarmongerChuna);
        }

        private void PhaseWarmongerTalarii()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatWarmongerTalarii);
        }

        private void PhaseGrandWarmongerTargresh()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatGrandWarmongerTargresh);//activate this once someone tiggers the portal
        }

        private void PhaseBetaElementalGuardians()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatTheElementalGuardians2);

            BroadcastCommunicatorMessage(CommunicatorMessage.Caretaker114);
        }

        private void PhaseBetaPersonalityDatacore()
        {
            publicEvent.ActivateObjective(PublicEventObjective.RetrieveTheThirdPersonalityDatacore);//I am guessing you add the objective based on how many wings you cleared, we need to fix this later, for now I will leave like this
        }

        private void PhaseMemoryCores()
        {
            publicEvent.ActivateObjective(PublicEventObjective.PlaceTheDatacoresInTheOculus);
        }

        private void PhaseAvatus()
        {
            foreach (IPlayer player in mapInstance.GetPlayers())
                player.CinematicManager.QueueCinematic(cinematicFactory.CreateCinematic<IDatascapeAvatusSpawn>());

            publicEvent.ActivateObjective(PublicEventObjective.DefeatAvatus);
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
                case PublicEventObjective.DefeatTheSystemDaemons:
                    publicEvent.SetPhase(PublicEventPhase.TheOculus);
                    break;
                case PublicEventObjective.DefeatOptimizedMemoryProbeP2Z://needs to look if System Daemons are killed or not
                    publicEvent.ActivateObjective(PublicEventObjective.EscapeTheLimboInfomatrix);
                    break;
                case PublicEventObjective.EscapeTheLimboInfomatrix:
                    publicEvent.SetPhase(PublicEventPhase.FirstFrostBoulder);
                    break;
                case PublicEventObjective.DefeatTheFirstFrostBoulderAvalanche:
                    publicEvent.SetPhase(PublicEventPhase.SecondFrostBoulder);
                    break;
                case PublicEventObjective.DefeatTheSecondFrostBoulderAvalanche:
                    publicEvent.SetPhase(PublicEventPhase.FrostbringerWarlock);
                    break;
                case PublicEventObjective.DefeatTheFrostbringerWarlock:
                    publicEvent.SetPhase(PublicEventPhase.MaelstromAuthority);
                    break;
                case PublicEventObjective.DefeatTheMaelstromAuthority:
                    publicEvent.SetPhase(PublicEventPhase.AlphaPersonalityDatacore);
                    break;
                case PublicEventObjective.DefeatOptimizedMemoryProbeED1://needs to look if System Daemons are killed or not
                    publicEvent.ActivateObjective(PublicEventObjective.DefeatTheBioEnhancedBroodmother);
                    break;
                case PublicEventObjective.DefeatTheBioEnhancedBroodmother:
                    publicEvent.SetPhase(PublicEventPhase.EarthRoomCanimid);
                    break;
                case PublicEventObjective.DefeatTheFullyOptimizedCanimid:
                    publicEvent.SetPhase(PublicEventPhase.EarthRoomRock);
                    break;
                case PublicEventObjective.DefeatTheLogicGuidedRockslide:
                    publicEvent.SetPhase(PublicEventPhase.Gloomclaw);
                    break;
                case PublicEventObjective.DefeatGloomclaw:
                    publicEvent.SetPhase(PublicEventPhase.LogicWingRoom1);
                    break;
                case PublicEventObjective.GeneratorCharge2://I am not sure how to do this
                    publicEvent.ActivateObjective(PublicEventObjective.DefeatTheHyperAcceleratedSkeledroid);
                    break;
                case PublicEventObjective.DefeatTheHyperAcceleratedSkeledroid:
                    publicEvent.ActivateObjective(PublicEventObjective.DefyPerspective);// needs to look if objective is active or not
                    break;
                case PublicEventObjective.PowerUpAllOfTheEldanPowerGenerators://look at sniffs
                    publicEvent.SetPhase(PublicEventPhase.LogicWingRoom2);
                    break;
                case PublicEventObjective.GeneratorCharge8:
                    publicEvent.ActivateObjective(PublicEventObjective.DefeatTheAugmentedHeraldOfAvatus);
                    break;
                case PublicEventObjective.PowerUpTheEldanPowerGenerators://look at sniffs
                    publicEvent.SetPhase(PublicEventPhase.LogicWingRoom3);
                    break;
                case PublicEventObjective.PowerUpAllOfTheEldanPowerGenerators3:
                    publicEvent.SetPhase(PublicEventPhase.LogicWingLogicElemental);
                    break;
                case PublicEventObjective.DefeatTheAbstractAugmentationAlgorithm://look how this works when you didn't kill the other minibosses
                    publicEvent.SetPhase(PublicEventPhase.DeltaElementalGuardians);
                    break;
                case PublicEventObjective.DefeatTheElementalGuardians3:
                    publicEvent.SetPhase(PublicEventPhase.DeltaPersonalityDatacore);
                    break;
                case PublicEventObjective.DefeatOptimizedMemoryProbeTX67://Needs to look if system daemons are killed or not
                    publicEvent.SetPhase(PublicEventPhase.VolatilityLattice);
                    break;
                case PublicEventObjective.EscapeAvatusAttention:
                    publicEvent.SetPhase(PublicEventPhase.WarmongerAgratha);
                    break;
                case PublicEventObjective.DefeatWarmongerAgratha:
                    publicEvent.SetPhase(PublicEventPhase.WarmongerChuna);
                    break;
                case PublicEventObjective.DefeatWarmongerChuna:
                    publicEvent.ActivateObjective(PublicEventPhase.WarmongerTalarii);
                    break;
                case PublicEventObjective.DefeatWarmongerTalarii:
                    publicEvent.SetPhase(PublicEventPhase.GrandWarmongerTargresh);
                    break;
                case PublicEventObjective.DefeatGrandWarmongerTargresh:
                    publicEvent.SetPhase(PublicEventPhase.BetaElementalGuardians);
                    break;
                case PublicEventObjective.DefeatTheElementalGuardians2:
                    publicEvent.SetPhase(PublicEventPhase.BetaPersonalityDatacore);
                    break;
                case PublicEventObjective.RetrieveTheFirstPersonalityDatacore:
                case PublicEventObjective.RetrieveTheSecondPersonalityDatacore:
                case PublicEventObjective.RetrieveTheThirdPersonalityDatacore:
                    publicEvent.SetPhase(PublicEventPhase.MemoryCores);
                    break;
                case PublicEventObjective.PlaceTheDatacoresInTheOculus:
                    publicEvent.SetPhase(PublicEventPhase.Avatus);
                    break;
                case PublicEventObjective.DefeatAvatus:
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

            }
        }
    }
}
