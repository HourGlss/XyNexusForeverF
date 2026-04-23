using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Cinematic.Cinematics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Trigger;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Static.PublicEvent;

namespace NexusForever.Script.Instance.Dungeon.StormtalonsLair
{
    [ScriptFilterOwnerId(145)]
    public class StormtalonsLairEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;
        private IMapInstance mapInstance;

        private enum Creature
        {
            NormalStormTalon  = 17163,
            VeteranStormTalon = 33406,
        }

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;
        private readonly IGlobalQuestManager globalQuestManager;

        public StormtalonsLairEventScript(
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
            mapInstance = publicEvent.Map as IMapInstance;

            //publicEvent.SetPhase(PublicEventPhase.Enter);
            publicEvent.SetPhase(PublicEventPhase.StopTheThundercallHighPriest);
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
                case PublicEventPhase.Enter:
                    OnPhaseEnter();
                    break;
                case PublicEventPhase.DefeatBlade_WindTheInvoker:
                    OnPhaseDefeatBlade_WindTheInvoker();
                    break;
                case PublicEventPhase.EliminateAethros:
                    OnPhaseEliminateAethros();
                    break;
                case PublicEventPhase.StopTheThundercallHighPriest:
                    OnPhaseStopTheThundercallHighPriest();
                    break;
                case PublicEventPhase.DestroyStormtalon:
                    OnPhaseDestroyStormtalon();
                    break;
            }
        }
        private void OnPhaseEnter()
        {
            Random rnd = new();
            uint[] Object = [0, 1, 2];
            int Index1 = rnd.Next(Object.Length);
            if (Index1 > 0) publicEvent.ActivateObjective(PublicEventObjective.GatherTaintedStemSamples);
        }
        private void OnPhaseDefeatBlade_WindTheInvoker()
        {
           publicEvent.ActivateObjective(PublicEventObjective.DefeatBlade_WindTheInvoker);
           Random rnd = new();
           uint[] Object = [0, 1];
           int Index1 = rnd.Next(Object.Length);
           if (Index1 > 0) publicEvent.ActivateObjective(PublicEventObjective.ObtainDataFromTheThundercallDataAltar);//is this after the boss?
           int Index2 = rnd.Next(Object.Length);
           if (Index2 > 0) publicEvent.ActivateObjective(PublicEventObjective.HijackPowerFromTheThundercallStormTotems);
        }

        private void OnPhaseEliminateAethros()
        {
            publicEvent.ActivateObjective(PublicEventObjective.EliminateAethros);

            Random rnd = new();
            uint[] Object = [0, 1];
            int Index1 = rnd.Next(Object.Length);
            if (Index1 > 0) publicEvent.ActivateObjective(PublicEventObjective.FreeTheThundercallSacrificialPrisoners);
            int Index2 = rnd.Next(Object.Length);
            if (Index2 > 0) publicEvent.ActivateObjective(PublicEventObjective.UseYourGrenadesToDisableTheThundercallPell);
            int Index3 = rnd.Next(Object.Length);
            if (Index3 > 0) publicEvent.ActivateObjective(PublicEventObjective.DefeatArcanistBreezeBinderForTheEncryptionKey);// or can we have both of them?
            else publicEvent.ActivateObjective(PublicEventObjective.KillOverseerDriftCatcher);
        }

        private void OnPhaseStopTheThundercallHighPriest()
        {
            publicEvent.ActivateObjective(PublicEventObjective.StopTheThundercallHighPriest, mapInstance.PlayerCount);

            var triggerEntity = publicEvent.CreateEntity<ITurnstileGridTriggerEntity>();
                triggerEntity.Initialise(1831, 15f, 1831);
                triggerEntity.AddToMap(mapInstance, new Vector3(114.215f, -17.1291f, 321.243f)); //is 50145 correct?
        }
        private void OnPhaseDestroyStormtalon()
        {
            foreach (IPlayer player in mapInstance.GetPlayers())
                player.CinematicManager.QueueCinematic(cinematicFactory.CreateCinematic<IStormtalonLairStormtalonReborn>());

            publicEvent.ActivateObjective(PublicEventObjective.DestroyStormtalon);

            //TODO Spawn Stormtalon based on dungeon level
        }

        private void BroadcastCommunicatorMessage(CommunicatorMessage message) // look on youtube, maybe we can delete this
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
                case PublicEventObjective.SurviveTheThundercallPellZealots:
                    publicEvent.SetPhase(PublicEventPhase.DefeatBlade_WindTheInvoker);
                    break;
                case PublicEventObjective.DefeatBlade_WindTheInvoker:
                    publicEvent.SetPhase(PublicEventPhase.EliminateAethros);
                    break;
                case PublicEventObjective.EliminateAethros:
                    publicEvent.SetPhase(PublicEventPhase.StopTheThundercallHighPriest);
                    break;
                case PublicEventObjective.StopTheThundercallHighPriest:
                    publicEvent.SetPhase(PublicEventPhase.DestroyStormtalon);
                    break;
                case PublicEventObjective.DestroyStormtalon:
                    publicEvent.Finish(PublicEventTeam.PublicTeam);
                    break;

            }
        }
    }
}
