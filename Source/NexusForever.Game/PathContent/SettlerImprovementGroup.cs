using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Entity;
using NexusForever.Game.Entity.Creature;
using NexusForever.Game.PathContent.Static;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model;
using NexusForever.Network.World.Message.Model.Shared;
using NexusForever.Shared;
using NexusForever.Shared.Game;
using System;
using System.Linq;

namespace NexusForever.Game.PathContent
{
    public class SettlerImprovementGroup : IUpdate
    {
        public PathSettlerImprovementGroupEntry Entry { get; private set; }
        public List<PathSettlerImprovementEntry> Tiers { get; private set; }
        public UpdateTimer expiryTimer = new(0, false);
        public bool PendingExpiry() => expiryTimer.HasElapsed;
        public bool Active { get; private set; }
        public int Tier { get; private set; } = -1;
        public List<string> Owners { get; set; } = [];
        public IWorldEntity Entity { get; private set; }

        public SettlerImprovementGroup(uint groupId)
        {
            Entry = GameTableManager.Instance.PathSettlerImprovementGroup.GetEntry(groupId);
            Tiers = GameTableManager.Instance.PathSettlerImprovement.Entries
                .Where(i => i.Id == Entry.PathSettlerImprovementIdTier00
                    || i.Id == Entry.PathSettlerImprovementIdTier01
                    || i.Id == Entry.PathSettlerImprovementIdTier02
                    || i.Id == Entry.PathSettlerImprovementIdTier03)
                .ToList();
        }

        public void Update(double lastTick)
        {
            if (!expiryTimer.IsTicking)
                return;

            expiryTimer.Update(lastTick);
            if (!expiryTimer.HasElapsed)
                return;

            Entity?.RemoveFromMap();
            Active = false;
            Tier = -1;
            Owners.Clear();
        }

        public void Build(Player player, int tier)
        {
            if (tier > 0)
                throw new NotImplementedException();

            Active = true;
            Tier = tier;
            expiryTimer = new UpdateTimer(expiryTimer.Time + Entry.DurationPerBundleMs / 1000d, true);

            player.PathMissionManager.MissionUpdate(PathMissionType.Settler_Hub, Entry.PathSettlerHubId, 1u);

            Owners.Add(player.Name);
            player.EnqueueToVisible(new ServerPathSettlerBuildStatusUpdate
            {
                PathSettlerHubId = (ushort)Entry.PathSettlerHubId,
                Status = GetNetworkBuildStatus()
            }, true);
            player.EnqueueToVisible(new ServerPathSettlerHubUpdate
            {
                PathSettlerHubId = Entry.PathSettlerHubId
            }, true);
            player.Session.EnqueueMessageEncrypted(new ServerPathSettlerBuildResult
            {
                eResult = 1,
                Unknown = (uint)Tiers[0].Id,
                PathSettlerImprovementGroupId = Entry.Id
            });

            ImprovementInfo info = GlobalPathContentManager.Instance.GetImprovementInfo(Entry.Id);
            Entity = CreateSimpleEntity(info.CreatureId, Entry.Id, info.DisplayInfo);
            player.Map.EnqueueAdd(Entity, info.Position);
        }

        public SettlerImprovementGroupStatus GetNetworkBuildStatus()
        {
            return new SettlerImprovementGroupStatus
            {
                PathSettlerImprovementGroupId = (ushort)Entry.Id,
                CurrentTier = Active ? (uint)Tier : uint.MaxValue,
                RemainingTime = Active ? (uint)(expiryTimer.Time * 1000d) : 0u,
                Unknown = Active ? (uint)(expiryTimer.Time * 1000d / Entry.DurationPerBundleMs) : 0u
            };
        }

        private static ISimpleEntity CreateSimpleEntity(uint creatureId, uint improvementGroupId, uint displayInfoId)
        {
            IEntityFactory entityFactory = LegacyServiceProvider.Provider.GetRequiredService<IEntityFactory>();
            ICreatureInfoManager creatureInfoManager = LegacyServiceProvider.Provider.GetRequiredService<ICreatureInfoManager>();

            ICreatureInfo creatureInfo = new CreatureInfoOverride()
                .SetCreatureInfoOverride(creatureInfoManager.GetCreatureInfo(creatureId))
                .SetDisplayInfoEntryOverride(GameTableManager.Instance.Creature2DisplayInfo.GetEntry(displayInfoId));

            ISimpleEntity entity = entityFactory.CreateEntity<ISimpleEntity>();
            entity.Initialise(creatureInfo);
            entity.ImprovementGroupId = improvementGroupId;
            return entity;
        }
    }
}
