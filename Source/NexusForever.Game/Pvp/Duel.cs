using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Pvp;
using NexusForever.Game.Static.PVP;
using NexusForever.Network.World.Message.Model.Pvp;
using NexusForever.Shared;
using NexusForever.Shared.Game;

namespace NexusForever.Game.Pvp
{
    internal class Duel : IUpdate
    {
        private const float FlagRange = 30f;
        private const double ChallengeDurationSeconds = 30d;
        private const double PreparationDurationSeconds = 5d;
        private const double RangeCheckIntervalSeconds = 0.5d;
        private const double OutOfRangeDurationSeconds = 10d;

        public ulong Id { get; }
        public IPlayer Challenger { get; }
        public IPlayer Recipient { get; }
        public ISimpleEntity Flag { get; }

        public DuelState State { get; private set; } = DuelState.Pending;
        public bool IsFinished => State == DuelState.Finished;

        private readonly PvPFlag challengerInitialPvpFlags;
        private readonly PvPFlag recipientInitialPvpFlags;

        private readonly UpdateTimer challengeTimer = new(ChallengeDurationSeconds);
        private readonly UpdateTimer preparationTimer = new(PreparationDurationSeconds, false);
        private readonly UpdateTimer rangeCheckTimer = new(RangeCheckIntervalSeconds);
        private readonly UpdateTimer challengerOutOfRangeTimer = new(OutOfRangeDurationSeconds, false);
        private readonly UpdateTimer recipientOutOfRangeTimer = new(OutOfRangeDurationSeconds, false);

        public Duel(ulong id, IPlayer challenger, IPlayer recipient, ISimpleEntity flag)
        {
            Id = id;
            Challenger = challenger;
            Recipient = recipient;
            Flag = flag;

            challengerInitialPvpFlags = challenger.PvPFlags;
            recipientInitialPvpFlags = recipient.PvPFlags;
        }

        public bool Contains(IPlayer player)
        {
            return Challenger == player || Recipient == player;
        }

        public bool IsPendingFor(IPlayer player)
        {
            return State == DuelState.Pending && Recipient == player;
        }

        public IPlayer GetOpponent(IPlayer player)
        {
            return Challenger == player ? Recipient : Challenger;
        }

        public void Update(double lastTick)
        {
            if (IsFinished)
                return;

            if (State == DuelState.Pending)
            {
                challengeTimer.Update(lastTick);
                if (challengeTimer.HasElapsed)
                    Cancel(DuelFinishReason.DuelCancelled);
                return;
            }

            if (State == DuelState.Preparing)
            {
                preparationTimer.Update(lastTick);
                if (preparationTimer.HasElapsed)
                    Start();
            }

            if (State is not DuelState.Preparing and not DuelState.InProgress)
                return;

            rangeCheckTimer.Update(lastTick);
            if (rangeCheckTimer.HasElapsed)
            {
                CheckFlagOutOfRange(Challenger, challengerOutOfRangeTimer);
                CheckFlagOutOfRange(Recipient, recipientOutOfRangeTimer);
                rangeCheckTimer.Reset();
            }

            challengerOutOfRangeTimer.Update(lastTick);
            if (challengerOutOfRangeTimer.HasElapsed)
            {
                Finish(Recipient, Challenger, DuelFinishReason.Forfeited1);
                return;
            }

            recipientOutOfRangeTimer.Update(lastTick);
            if (recipientOutOfRangeTimer.HasElapsed)
                Finish(Challenger, Recipient, DuelFinishReason.Forfeited1);
        }

        public void Accept()
        {
            if (State != DuelState.Pending)
                return;

            Challenger.DuelOpponentGuid = Recipient.Guid;
            Recipient.DuelOpponentGuid = Challenger.Guid;

            SendToParticipants(new ServerDuelStart
            {
                ChallengerUnitId = Challenger.Guid,
                OpponentUnitId = Recipient.Guid
            });
            SendToParticipants(new ServerDuelCountdown
            {
                ChallengerUnitId = Challenger.Guid,
                OpponentUnitId = Recipient.Guid
            });

            preparationTimer.Reset();
            State = DuelState.Preparing;
        }

        public void Decline()
        {
            if (State != DuelState.Pending)
                return;

            Cancel(DuelFinishReason.DeclinedRequest);
        }

        public void Forfeit(IPlayer forfeitingPlayer)
        {
            if (State is not DuelState.Preparing and not DuelState.InProgress)
                return;

            Finish(GetOpponent(forfeitingPlayer), forfeitingPlayer, DuelFinishReason.Forfeited2);
        }

        public void FinishForDeath(IPlayer loser)
        {
            if (State is not DuelState.Preparing and not DuelState.InProgress)
                return;

            Finish(GetOpponent(loser), loser, DuelFinishReason.Defeated);
        }

        public void Cancel(DuelFinishReason reason)
        {
            if (IsFinished)
                return;

            Challenger.DuelOpponentGuid = null;
            Recipient.DuelOpponentGuid = null;

            SendToParticipants(new ServerDuelResult
            {
                WinnerUnitId = 0u,
                LoserUnitId = 0u,
                Reason = reason
            });

            RemoveFlag();
            State = DuelState.Finished;
        }

        private void Start()
        {
            if (State != DuelState.Preparing)
                return;

            Challenger.PvPFlags |= PvPFlag.Enabled;
            Recipient.PvPFlags |= PvPFlag.Enabled;

            Challenger.ThreatManager.UpdateThreat(Recipient, 1);
            Recipient.ThreatManager.UpdateThreat(Challenger, 1);

            State = DuelState.InProgress;
        }

        private void Finish(IPlayer winner, IPlayer loser, DuelFinishReason reason)
        {
            if (IsFinished)
                return;

            winner.DuelOpponentGuid = null;
            loser.DuelOpponentGuid = null;

            SendToParticipants(new ServerDuelResult
            {
                WinnerUnitId = winner.Guid,
                LoserUnitId = loser.Guid,
                Reason = reason
            });

            RestorePvPState();
            ClearThreat();
            RemoveFlag();

            State = DuelState.Finished;
        }

        private void RestorePvPState()
        {
            Challenger.PvPFlags = challengerInitialPvpFlags.HasFlag(PvPFlag.Enabled)
                ? Challenger.PvPFlags
                : Challenger.PvPFlags & ~PvPFlag.Enabled;

            Recipient.PvPFlags = recipientInitialPvpFlags.HasFlag(PvPFlag.Enabled)
                ? Recipient.PvPFlags
                : Recipient.PvPFlags & ~PvPFlag.Enabled;
        }

        private void ClearThreat()
        {
            Challenger.ThreatManager.RemoveHostile(Recipient.Guid);
            Recipient.ThreatManager.RemoveHostile(Challenger.Guid);
        }

        private void RemoveFlag()
        {
            if (Flag.InWorld)
                Flag.RemoveFromMap();
        }

        private void CheckFlagOutOfRange(IPlayer player, UpdateTimer timer)
        {
            bool isOutOfRange = Vector3.Distance(player.Position, Flag.Position) > FlagRange;

            if (!isOutOfRange)
            {
                if (timer.IsTicking)
                {
                    player.Session.EnqueueMessageEncrypted(new ServerDuelCancelWarning());
                    timer.Reset(false);
                }

                return;
            }

            if (!timer.IsTicking)
            {
                player.Session.EnqueueMessageEncrypted(new ServerDuelLeftArea());
                timer.Reset();
            }
        }

        private void SendToParticipants<T>(T message) where T : class, Network.Message.IWritable
        {
            Challenger.Session.EnqueueMessageEncrypted(message);
            if (Recipient.Session != Challenger.Session)
                Recipient.Session.EnqueueMessageEncrypted(message);
        }
    }
}
