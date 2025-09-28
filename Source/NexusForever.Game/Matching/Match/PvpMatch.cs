using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.Matching;
using NexusForever.Game.Abstract.Matching.Match;
using NexusForever.Game.Abstract.Matching.Queue;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Matching;
using NexusForever.Game.Static.PVP;
using NexusForever.GameTable;
using NexusForever.Network.Internal;
using NexusForever.Network.World.Message.Model;
using NexusForever.Network.World.Message.Model.Shared;
using NexusForever.Shared;
using NexusForever.Shared.Game;

namespace NexusForever.Game.Matching.Match
{
    public class PvpMatch : Match, IPvpMatch
    {
        private PvpGameState state;
        private UpdateTimer stateTimer;
        private Action stateCallback;

        // only deathpool stats are tracked in the match, other types are tracked in the public events
        // looks like this is legacy and arenas never got migrated to the new public event system
        private readonly Dictionary<Static.Matching.MatchTeam, uint> deathmatchPool = [];

        #region Dependency Injection

        private readonly IPlayerManager playerManager;

        public PvpMatch(
            ILogger<PvpMatch> log,
            IMatchManager matchManager,
            IMatchingDataManager matchingDataManager,
            IFactory<IMatchTeam> matchTeamFactory,
            IGameTableManager gameTableManager,
            IPlayerManager playerManager,
            IInternalMessagePublisher messagePublisher)
            : base(log, matchManager, matchingDataManager, matchTeamFactory, gameTableManager, playerManager, messagePublisher)
        {
            this.playerManager = playerManager;
        }

        #endregion

        /// <summary>
        /// Initialise the match with the supplied <see cref="IMatchProposal"/>
        /// </summary>
        public override void Initialise(IMatchProposal matchProposal)
        {
            base.Initialise(matchProposal);

            if (MatchingMap.GameTypeEntry.MatchingRulesEnum == MatchRules.DeathmatchPool)
                foreach (IMatchTeam team in GetTeams())
                    deathmatchPool.Add(team.Team, MatchingMap.GameTypeEntry.MatchingRulesData01);

            SetState(PvpGameState.Initialized);
        }

        protected override void InitialiseTeams(IMatchProposal matchProposal)
        {
            // for PvP matches, the team is randomly selected
            Static.Matching.MatchTeam team = (Static.Matching.MatchTeam)Random.Shared.Next(0, 2);
            foreach (IMatchingQueueGroupTeam matchingQueueGroupTeam in matchProposal.MatchingQueueGroup.GetTeams())
            {
                InitialiseTeam(team, matchingQueueGroupTeam);
                team = team == Static.Matching.MatchTeam.Blue ? Static.Matching.MatchTeam.Red : Static.Matching.MatchTeam.Blue;
            }
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public override void Update(double lastTick)
        {
            if (stateTimer != null)
            {
                stateTimer.Update(lastTick);
                if (stateTimer.HasElapsed && stateCallback != null)
                {
                    stateTimer = null;
                    stateCallback();
                }
            }

            base.Update(lastTick);
        }

        /// <summary>
        /// Set the state of the match to the supplied <see cref="PvpGameState"/>.
        /// </summary>
        public void SetState(PvpGameState state)
        {
            this.state = state;

            switch (state)
            {
                // wait 10 seconds or until all players have entered the match to start preparation, whichever happens first
                case PvpGameState.Initialized:
                    SetNextPhase(10000, () => { SetState(PvpGameState.Preparation); });
                    break;
                case PvpGameState.Preparation:
                    SetNextPhase(MatchingMap.GameTypeEntry.PreparationTimeMS, () => { SetState(PvpGameState.InProgress); });
                    break;
                case PvpGameState.InProgress:
                    OnPhaseInProgress();
                    break;
                case PvpGameState.Finished:
                    OnPhaseFinished();
                    break;
            }

            Broadcast(new ServerMatchingMatchPvpStateUpdated
            {
                State = state,
            });

            if (map is IContentPvpMapInstance pvpMapInstance)
                pvpMapInstance.OnPvpMatchState(state);
        }

        private void SetNextPhase(uint time, Action callback)
        {
            if (time == 0)
                return;

            stateTimer    = new UpdateTimer(TimeSpan.FromMilliseconds(time));
            stateCallback = callback;
        }

        private void OnPhaseInProgress()
        {
            SetNextPhase(MatchingMap.GameTypeEntry.MatchTimeMS, () => { MatchFinish(MatchWinner.Draw, MatchEndReason.TimeExpired); });

            foreach (IMatchTeam matchTeam in GetTeams())
            {
                foreach (IMatchTeamMember matchTeamMember in matchTeam.GetMembers())
                {
                    IPlayer player = playerManager.GetPlayer(matchTeamMember.Identity);
                    if (player == null)
                        continue;

                    EnableForcedPvp(player);
                }
            }
        }

        private void OnPhaseFinished()
        {
            stateTimer    = null;
            stateCallback = null;

            foreach (IMatchTeam matchTeam in GetTeams())
            {
                foreach (IMatchTeamMember matchTeamMember in matchTeam.GetMembers())
                {
                    IPlayer player = playerManager.GetPlayer(matchTeamMember.Identity);
                    if (player == null)
                        continue;

                    RemoveForcedPvp(player);
                }
            }
        }

        /// <summary>
        /// Update deathmatch pool for the team the character is on.
        /// </summary>
        public void UpdatePool(Abstract.Identity identity)
        {
            if (MatchingMap.GameTypeEntry.MatchingRulesEnum != MatchRules.DeathmatchPool)
                return;

            IMatchTeam team = GetTeam(identity);
            if (team == null)
                throw new InvalidOperationException();

            if (deathmatchPool[team.Team] == 0)
                throw new InvalidOperationException();

            deathmatchPool[team.Team]--;
            SendPoolUpdate();
        }

        private void SendPoolUpdate()
        {
            Broadcast(new ServerMatchingMatchPvpPoolUpdated
            {
                LivesRemainingTeam1 = deathmatchPool[Static.Matching.MatchTeam.Red],
                LivesRemainingTeam2 = deathmatchPool[Static.Matching.MatchTeam.Blue]
            });
        }

        /// <summary>
        /// Invoked when <see cref="IPlayer"/> enters the match.
        /// </summary>
        public override void MatchEnter(IPlayer player)
        {
            base.MatchEnter(player);

            IMatchTeam team = GetTeam(player.Identity);
            if (team == null)
                throw new InvalidOperationException();

            player.Session.EnqueueMessageEncrypted(new ServerMatchingMatchPvpStateInitial
            {
                Team  = team.Team,
                State = new StateInfo
                {
                    State       = state,
                    TimeElapsed = (uint)TimeSpan.FromSeconds(stateTimer.Duration - stateTimer.Time).TotalMilliseconds
                }
            });

            if (MatchingMap.GameTypeEntry.MatchingRulesEnum == MatchRules.DeathmatchPool)
                SendPoolUpdate();

            if (state == PvpGameState.InProgress)
                EnableForcedPvp(player);
        }

        private void EnableForcedPvp(IPlayer player)
        {
            player.PvPFlags |= PvPFlag.Forced;
            player.ResurrectionManager.CanResurrectOtherPlayer = false;
            player.SetControl(player);
        }

        /// <summary>
        /// Invoked when <see cref="IPlayer"/> exits the match.
        /// </summary>
        public override void MatchExit(IPlayer player, bool teleport)
        {
            base.MatchExit(player, teleport);

            if (state == PvpGameState.InProgress)
                RemoveForcedPvp(player);
        }

        private void RemoveForcedPvp(IPlayer player)
        {
            player.PvPFlags &= ~PvPFlag.Forced;
            player.ResurrectionManager.CanResurrectOtherPlayer = true;
        }

        /// <summary>
        /// Remove character from match.
        /// </summary>
        public override void MatchLeave(Abstract.Identity identity)
        {
            IMatchTeam team = GetTeam(identity);
            if (team == null)
                throw new InvalidOperationException();

            base.MatchLeave(identity);

            // if all members of a team have left, the other team wins
            if (Status == MatchStatus.InProgress)
                if (!team.GetMembers().Any())
                    MatchFinish(team.Team == Static.Matching.MatchTeam.Red ? MatchWinner.Blue : MatchWinner.Red, MatchEndReason.Completed);
        }

        /// <summary>
        /// Finish the match with the supplied <see cref="MatchWinner"/> and <see cref="MatchEndReason"/>
        /// </summary>
        public void MatchFinish(MatchWinner matchWinner, MatchEndReason matchEndReason)
        {
            if (Status != MatchStatus.InProgress)
                return;

            MatchFinish();

            SetState(PvpGameState.Finished);

            Broadcast(new ServerMatchingMatchPvpFinished
            {
                Winner = matchWinner,
                Reason = matchEndReason
            });

            if (map is IContentPvpMapInstance pvpMapInstance)
                pvpMapInstance.OnPvpMatchFinish(matchWinner, matchEndReason);
        }

        /// <summary>
        /// Invoked when <see cref="IPlayer"/> dies.
        /// </summary>
        public void OnDeath(IPlayer player)
        {
            IMatchTeam team = GetTeam(player.Identity);
            if (team == null)
                throw new InvalidOperationException();

            // if the match has a deathmatch pool and the team has no lives left, the other team wins
            if (DeathpoolDepleted(team))
            {
                MatchFinish(team.Team == Static.Matching.MatchTeam.Red ? MatchWinner.Blue : MatchWinner.Red, MatchEndReason.Completed);
                return;
            }

            bool canResurrect = CanResurrect(team);
            ResurrectionType type = canResurrect ? map.GetResurrectionType() : ResurrectionType.None;

            // TODO: should the timespans be dynamic?
            player.ResurrectionManager.ShowResurrection(type, canResurrect ? TimeSpan.FromSeconds(5) : null, canResurrect ? TimeSpan.FromSeconds(20) : null);
        }

        private bool DeathpoolDepleted(IMatchTeam team)
        {
            if (MatchingMap.GameTypeEntry.MatchingRulesEnum != MatchRules.DeathmatchPool)
                return false;

            if (deathmatchPool[team.Team] > 0)
                return false;

            foreach (IMatchTeamMember matchTeamMember in team.GetMembers())
            {
                if (!matchTeamMember.InMatch)
                    continue;

                IPlayer player = playerManager.GetPlayer(matchTeamMember.Identity);
                if (player == null)
                    continue;

                if (player.IsAlive)
                    return false;
            }

            return true;
        }

        private bool CanResurrect(IMatchTeam team)
        {
            if (MatchingMap.GameTypeEntry.MatchingRulesEnum != MatchRules.DeathmatchPool)
                return true;

            return deathmatchPool[team.Team] > 0;
        }
    }
}
