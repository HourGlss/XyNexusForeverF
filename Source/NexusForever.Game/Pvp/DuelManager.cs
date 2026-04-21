using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Entity;
using NexusForever.Game.Entity.Creature;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Pvp;
using NexusForever.Game.Static.Reputation;
using NexusForever.GameTable;
using NexusForever.Network.World.Message.Model.Pvp;
using NexusForever.Shared;
using NLog;

namespace NexusForever.Game.Pvp
{
    public class DuelManager : Singleton<DuelManager>, IUpdate
    {
        private const float MaxChallengeDistance = 30f;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<ulong, Duel> duels = [];
        private ulong nextDuelId = 1ul;

        public void Update(double lastTick)
        {
            foreach ((ulong duelId, Duel duel) in duels.ToArray())
            {
                duel.Update(lastTick);
                if (duel.IsFinished)
                    duels.Remove(duelId);
            }
        }

        public void HandleRequest(IPlayer challenger)
        {
            if (challenger.TargetGuid == null)
            {
                SendFailure(challenger, DuelFailureReason.InvalidDuelTarget);
                return;
            }

            IPlayer recipient = challenger.GetVisible<IPlayer>(challenger.TargetGuid.Value);
            if (recipient == null)
            {
                SendFailure(challenger, DuelFailureReason.PlayerInAnotherPhase);
                return;
            }

            if (!CanCreateChallenge(challenger, recipient, out DuelFailureReason? failureReason))
            {
                if (failureReason.HasValue)
                    SendFailure(challenger, failureReason.Value);
                return;
            }

            ISimpleEntity flag = SummonFlag(challenger, recipient);
            if (flag == null)
            {
                log.Warn("Failed to create duel flag for {0} and {1}.", challenger.Name, recipient.Name);
                SendFailure(challenger, DuelFailureReason.CannotDuelHere);
                return;
            }

            var duel = new Duel(nextDuelId++, challenger, recipient, flag);
            duels.Add(duel.Id, duel);

            challenger.EnqueueToVisible(new ServerDuelChallenge
            {
                ChallengerUnitId = challenger.Guid,
                OpponentUnitId = recipient.Guid
            }, true);
        }

        public void HandleAccept(IPlayer recipient)
        {
            Duel duel = duels.Values.FirstOrDefault(d => d.IsPendingFor(recipient));
            if (duel == null)
            {
                SendFailure(recipient, DuelFailureReason.YouHaveDuelRequestPending);
                return;
            }

            duel.Accept();
        }

        public void HandleDecline(IPlayer recipient)
        {
            Duel duel = duels.Values.FirstOrDefault(d => d.IsPendingFor(recipient));
            duel?.Decline();
        }

        public void HandleForfeit(IPlayer player)
        {
            Duel duel = GetByParticipant(player);
            duel?.Forfeit(player);
        }

        public void EndDuelsForPlayer(IPlayer player, DuelFinishReason reason = DuelFinishReason.DuelCancelled)
        {
            Duel duel = GetByParticipant(player);
            duel?.Cancel(reason);
        }

        public void OnPlayerDeath(IPlayer player)
        {
            Duel duel = GetByParticipant(player);
            duel?.FinishForDeath(player);
        }

        private Duel GetByParticipant(IPlayer player)
        {
            return duels.Values.FirstOrDefault(d => d.Contains(player));
        }

        private static void SendFailure(IPlayer player, DuelFailureReason reason)
        {
            player.Session.EnqueueMessageEncrypted(new ServerDuelFailure
            {
                Reason = reason
            });
        }

        private static bool CanCreateChallenge(IPlayer challenger, IPlayer recipient, out DuelFailureReason? failureReason)
        {
            failureReason = null;

            if (challenger == recipient)
            {
                failureReason = DuelFailureReason.InvalidDuelTarget;
                return false;
            }

            if (!challenger.IsAlive)
            {
                failureReason = DuelFailureReason.YouCannotDuelWhileDead;
                return false;
            }

            if (!recipient.IsAlive)
            {
                failureReason = DuelFailureReason.CannotDuelDeadPlayer;
                return false;
            }

            if (challenger.InCombat)
            {
                failureReason = DuelFailureReason.YouAreInCombat;
                return false;
            }

            if (recipient.InCombat)
            {
                failureReason = DuelFailureReason.PlayerIsInCombat;
                return false;
            }

            if (challenger.IsDueling)
            {
                failureReason = DuelFailureReason.YouAreAlreadyDueling;
                return false;
            }

            if (recipient.IsDueling)
            {
                failureReason = DuelFailureReason.PlayerIsAlreadyDueling;
                return false;
            }

            if (recipient.IgnoreDuelRequests)
            {
                failureReason = DuelFailureReason.PlayerIsIgnoringDuels;
                return false;
            }

            if (challenger.Faction1 != recipient.Faction1)
            {
                failureReason = DuelFailureReason.CanOnlyDuelSameFaction;
                return false;
            }

            if (Vector3.Distance(challenger.Position, recipient.Position) > MaxChallengeDistance)
            {
                failureReason = DuelFailureReason.TooFarAway;
                return false;
            }

            return true;
        }

        private static ISimpleEntity SummonFlag(IPlayer challenger, IPlayer recipient)
        {
            uint creatureId = challenger.Faction1 switch
            {
                NexusForever.Game.Static.Reputation.Faction.Dominion => 47130u,
                NexusForever.Game.Static.Reputation.Faction.Exile => 47128u,
                _ => 0u
            };
            if (creatureId == 0u)
                return null;

            IEntityFactory entityFactory = LegacyServiceProvider.Provider.GetRequiredService<IEntityFactory>();
            ICreatureInfoManager creatureInfoManager = LegacyServiceProvider.Provider.GetRequiredService<ICreatureInfoManager>();

            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(creatureId);
            if (creatureInfo == null)
                return null;

            ISimpleEntity flag = entityFactory.CreateEntity<ISimpleEntity>();
            flag.Initialise(creatureInfo);
            flag.CreateFlags = EntityCreateFlag.SpawnAnimation;

            Vector3 position = (challenger.Position + recipient.Position) / 2f;
            challenger.Map.EnqueueAdd(flag, position);
            return flag;
        }
    }
}
