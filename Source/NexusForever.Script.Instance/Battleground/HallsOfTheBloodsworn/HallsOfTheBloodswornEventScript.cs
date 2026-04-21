using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Static.Matching;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Script.Template;

namespace NexusForever.Script.Instance.Battleground.HallsOfTheBloodsworn
{
    public abstract class HallsOfTheBloodswornEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;

        private bool redTeamWonRound1;
        private bool redTeamWonRound2;
        private bool blueTeamWonRound1;
        private bool blueTeamWonRound2;

        private readonly List<uint> doorEntities = [];

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IPublicEvent owner)
        {
            publicEvent = owner;
            publicEvent.SetPhase(PublicEventPhase.Preperation);
        }

        /// <summary>
        /// Invoked when the public event phase changes.
        /// </summary>
        public void OnPublicEventPhase(uint phase)
        {
            switch ((PublicEventPhase)phase)
            {
                case PublicEventPhase.Round1:
                    OnPhaseRound1();
                    break;
                case PublicEventPhase.Preperation2:
                    OnPhasePreperation2();
                    break;
                case PublicEventPhase.Round2:
                    OnPhaseRound2();
                    break;
                case PublicEventPhase.Preperation3:
                    OnPhasePreperation3();
                    break;
                case PublicEventPhase.Round3:
                    OnPhaseRound3();
                    break;
            }
        }

        private void OnPhaseRound1()
        {
            foreach (uint doorGuid in doorEntities)
            {
                IDoorEntity door = publicEvent?.Map.GetEntity<IDoorEntity>(doorGuid);
                door?.OpenDoor();
            }
        }

        private void OnPhasePreperation2()
        {
            publicEvent.ActivateObjective(PublicEventObjective.PrepareForBattle2);
        }

        private void OnPhaseRound2()
        {
            publicEvent.ActivateObjective(PublicEventObjective.FightForControl2);
        }

        private void OnPhasePreperation3()
        {
            publicEvent.ActivateObjective(PublicEventObjective.PrepareForBattle3);
        }

        private void OnPhaseRound3()
        {
            publicEvent.ActivateObjective(PublicEventObjective.FightForControl3);
        }

        /// <summary>
        /// Invoked when a PvP match <see cref="PvpGameState"/> changes on the same map the public event is on. 
        /// </summary>
        public void OnMatchState(PvpGameState state)
        {
            switch (state)
            {
                case PvpGameState.InProgress:
                    publicEvent.SetPhase(PublicEventPhase.Round1);
                    break;
                case PvpGameState.Finished:
                    publicEvent.SetPhase(PublicEventPhase.Finished);
                    break;
            }
        }

        /// <summary>
        /// Invoked when a <see cref="IGridEntity"/> is added to the map the public event is on.
        /// </summary>
        public void OnAddToMap(IGridEntity entity)
        {
            if (entity is not IWorldEntity worldEntity)
                return;

            switch ((PublicEventCreature)worldEntity.CreatureId)
            {
                case PublicEventCreature.HallsDoorBlueTeam:
                case PublicEventCreature.HallsDoorRedTeam:
                    doorEntities.Add(worldEntity.Guid);
                    break;
            }
        }

        public void OnPublicEventObjectiveStatus(IPublicEventObjective objective)
        {
            if (objective.Status != PublicEventStatus.Succeeded)
                return;

            switch ((PublicEventObjective)objective.Entry.Id)
            {
                case PublicEventObjective.PrepareForBattle:
                    publicEvent.SetPhase(PublicEventPhase.Round1);
                    break;
                case PublicEventObjective.FightForControl:
                    publicEvent.SetPhase(PublicEventPhase.Preperation2);
                    break;
                case PublicEventObjective.PrepareForBattle2:
                    publicEvent.SetPhase(PublicEventPhase.Round2);
                    break;
                case PublicEventObjective.FightForControl2:
                    if (redTeamWonRound1 && blueTeamWonRound2)
                    {
                        publicEvent.SetPhase(PublicEventPhase.Preperation3);
                    }
                else if (blueTeamWonRound1 && redTeamWonRound2)
                    {
                        publicEvent.SetPhase(PublicEventPhase.Preperation3);
                    }
                    else 
                    {
                        publicEvent.Finish(PublicEventTeam.PublicTeam);
                    }
                    break;
                case PublicEventObjective.PrepareForBattle3:
                    publicEvent.SetPhase(PublicEventPhase.Round3);
                    break;
                case PublicEventObjective.FightForControl3:
                    publicEvent.Finish(PublicEventTeam.PublicTeam);
                    break;
            }
        }

        public void OnDeath(IUnitEntity entity)
        {
            // TODO: resurrection timer and auto release
            // TODO: spawn flag
        }
    }
}