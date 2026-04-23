using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Trigger;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.EventInstances.ShadesEve
{
    [ScriptFilterOwnerId(597)]
    public class ShadesEveMainEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;
        private IMapInstance mapInstance;

        private uint gatherRingGuid;

        private uint townGateGuid;

        #region Dependency Injection

        private readonly IGlobalQuestManager globalQuestManager;

        public ShadesEveMainEventScript(
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
            publicEvent.SetPhase(PublicEventPhase.FindTheFountain);

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
            }
        }

        private void OnAddToMapWorldEntity(IWorldEntity worldEntity)
        {
            switch ((PublicEventCreature)worldEntity.CreatureId)
            {
                case PublicEventCreature.GatherRing1:
                    gatherRingGuid = worldEntity.Guid;
                    break;
                case PublicEventCreature.TownGate:
                    townGateGuid = worldEntity.Guid;
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
            }
        }

        private void OnRemoveFromMapWorldEntity(IWorldEntity worldEntity)
        {
            switch ((PublicEventCreature)worldEntity.CreatureId)
            {
                case PublicEventCreature.GatherRing1:
                    gatherRingGuid = 0;
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
                case PublicEventPhase.FindTheFountain:
                    OnPhaseFindTheFountain();
                    break;
                case PublicEventPhase.SpeakWithEttyWindsen:
                    OnPhaseSpeakWithEttyWindsen();
                    break;
                case PublicEventPhase.SpeakWithTheMayor:
                    OnPhaseSpeakWithTheMayor();
                    break;
                case PublicEventPhase.TalkWithTheLocals:
                    OnPhaseTalkWithTheLocals();
                    break;
            }
        }

        private void OnPhaseFindTheFountain()
        {
            publicEvent.ActivateObjective(PublicEventObjective.FindTheFountain, mapInstance.PlayerCount);

            var triggerEntity = publicEvent.CreateEntity<ITurnstileGridTriggerEntity>();//We may need a new trigger type, I can't get any of them to work
            triggerEntity.Initialise(2705, 40f, 7460);//fix the float
            triggerEntity.AddToMap(mapInstance, new Vector3(333.41528f, -871.36364f, -242.43496f));
        }

        private void OnPhaseSpeakWithEttyWindsen()
        {
            BroadcastCommunicatorMessage(CommunicatorMessage.TheAngel5);

            publicEvent.ActivateObjective(PublicEventObjective.SpeakWithEttyWindsen);

            var gatherRing = mapInstance.GetEntity<IWorldEntity>(gatherRingGuid);
            gatherRing?.RemoveFromMap();

            IDoorEntity door = mapInstance.GetEntity<IDoorEntity>(townGateGuid);
            door?.OpenDoor();
        }

        private void OnPhaseSpeakWithTheMayor()
        {
            publicEvent.ActivateObjective(PublicEventObjective.SpeakWithTheMayor);
        }

        private void OnPhaseTalkWithTheLocals()
        {
            publicEvent.ActivateObjective(PublicEventObjective.TalkWithTheLocals);
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
                case PublicEventObjective.FindTheFountain:
                    publicEvent.SetPhase(PublicEventPhase.SpeakWithEttyWindsen);
                    break;
                case PublicEventObjective.SpeakWithEttyWindsen:
                    publicEvent.SetPhase(PublicEventPhase.SpeakWithTheMayor);
                    break;
                case PublicEventObjective.SpeakWithTheMayor:
                    publicEvent.SetPhase(PublicEventPhase.TalkWithTheLocals);
                    break;
                case PublicEventObjective.TalkWithTheLocals:
                    publicEvent.StartVote(PublicEventTeam.PublicTeam, 64, 1);//get real default
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
                case PublicEventPhase.FindTheFountain:
                    SendCommunicatorMessage(player, CommunicatorMessage.TheAngel1);
                    break;
            }
        }
    }
}
