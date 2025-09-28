using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Static.Matching;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Script.Template;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Arena
{
    public abstract class ArenaSubEventScript<TEnum> : IPublicEventScript, IOwnedScript<IPublicEvent>
        where TEnum : Enum
    {
        public abstract TEnum PrepareForBattleObjective { get; }
        public abstract TEnum ParticipateInArenaObjective { get; }

        private IPublicEvent publicEvent;

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
                case PublicEventPhase.Fight:
                    publicEvent.ActivateObjective(ParticipateInArenaObjective);
                    break;
            }
        }

        /// <summary>
        /// Invoked when the <see cref="IPublicEventObjective"/> status changes.
        /// </summary>
        public void OnPublicEventObjectiveStatus(IPublicEventObjective objective)
        {
            if (objective.Status != PublicEventStatus.Succeeded)
                return;

            if (objective.Entry.Id == PrepareForBattleObjective.As<TEnum, uint>())
                publicEvent.SetPhase(PublicEventPhase.Fight);
        }

        /// <summary>
        /// Invoked when a PvP match <see cref="PvpGameState"/> changes on the same map the public event is on. 
        /// </summary>
        public void OnMatchState(PvpGameState state)
        {
            switch (state)
            {
                case PvpGameState.InProgress:
                    publicEvent.UpdateObjective(PrepareForBattleObjective, 0);
                    break;
            }
        }
    }
}
