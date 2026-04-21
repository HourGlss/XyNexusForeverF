using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Entity;
using NexusForever.Game.Entity.Creature;
using NexusForever.Game.PathContent.Static;
using NexusForever.Game.Static.Entity;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model;
using NexusForever.Shared;
using NexusForever.Shared.Game;
using System;
using System.Linq;
using System.Numerics;
using Path = NexusForever.Game.Static.Entity.Path;

namespace NexusForever.Game.PathContent
{
    public class GlobalPathContentManager : Singleton<GlobalPathContentManager>, IUpdate
    {
        // TODO: Do we need to save Improvement Group State during server reboot/crash?
        private readonly Dictionary<uint, SettlerImprovementGroup> settlerImprovementGroups = new();
        private readonly Dictionary<uint, ImprovementInfo> improvementInfo = new();

        public void Initialise()
        {
            // improvementInfo.Add(468, new ImprovementInfo()); // Royal Collegium
            improvementInfo.Add(469, new ImprovementInfo(new Vector3(-3480.7605f, -976.58154f, -6060.8135f), 25867, 25593)); // Shield Capacity Booster
            improvementInfo.Add(470, new ImprovementInfo(new Vector3(-3473f, -977.09224f, -6070.1f), 25716, 25593)); // Physical Resistance
            improvementInfo.Add(2620, new ImprovementInfo(new Vector3(-3489.43f, -976.972f, -6074.117f), 18838, 25612)); // Bank
        }

        public void Update(double lastTick)
        {
            foreach (SettlerImprovementGroup group in settlerImprovementGroups.Values)
                group.Update(lastTick);
        }

        public ImprovementInfo GetImprovementInfo(uint groupId)
        {
            return improvementInfo.TryGetValue(groupId, out ImprovementInfo info) ? info : null;
        }

        public void OnEnterZone(Player player)
        {
            if (player.Path == Path.Settler)
                SettlerSendImprovementBuildStatus(player);
        }

        public void OnEntityInteract(Player player, IWorldEntity target, uint interactionEvent)
        {
            if (interactionEvent != 30 || player.Path != Path.Settler)
                return;

            List<PathSettlerImprovementGroupEntry> improvementGroups = GameTableManager.Instance.PathSettlerImprovementGroup.Entries
                .Where(i => i.Creature2IdDepot == target.CreatureId)
                .ToList();
            if (improvementGroups.Count == 0)
                return;

            PathSettlerHubEntry hub = GameTableManager.Instance.PathSettlerHub.GetEntry(improvementGroups.First().PathSettlerHubId);

            var buildStatus = new ServerPathSettlerBuildStatusUpdateList
            {
                PathSettlerHubId = (ushort)hub.Id
            };

            foreach (PathSettlerImprovementGroupEntry groupEntry in GameTableManager.Instance.PathSettlerImprovementGroup.Entries
                .Where(g => g.PathSettlerHubId == hub.Id))
            {
                if (!settlerImprovementGroups.ContainsKey(groupEntry.Id))
                    settlerImprovementGroups.Add(groupEntry.Id, new SettlerImprovementGroup(groupEntry.Id));

                buildStatus.SettlerImprovementStatuses.Add(settlerImprovementGroups[groupEntry.Id].GetNetworkBuildStatus());
            }

            player.Session.EnqueueMessageEncrypted(buildStatus);
            player.Session.EnqueueMessageEncrypted(new ServerPathInvokeSettlerBuild
            {
                SettlerHubUnitId = target.Guid,
                PathSettlerImprovementGroupIds = improvementGroups.Select(i => i.Id).ToArray()
            });
        }

        public void OnAddVisible(Player player, ISimpleEntity target)
        {
            if (target.ImprovementGroupId == 0 || !settlerImprovementGroups.TryGetValue(target.ImprovementGroupId, out SettlerImprovementGroup group))
                return;

            player.Session.EnqueueMessageEncrypted(new ServerPathSettlerAddImprovementInfoToUnit
            {
                UnitId = target.Guid,
                PathSettlerImprovementGroupId = target.ImprovementGroupId,
                RemainingTime = (uint)(group.expiryTimer.Time * 1000d),
                Owners = group.Owners,
                Improvements = group.Owners.Select(owner => new ServerPathSettlerAddImprovementInfoToUnit.ImprovementInfo
                {
                    Name = owner,
                    Tier = 0
                }).ToList()
            });
        }

        /// <summary>
        /// Handles an explorer signal placement packet.
        /// </summary>
        public void HandleExplorerPlaceSignal(Player player, ClientPathExplorerProgressReport placeSignal)
        {
            PathMissionEntry missionEntry = GameTableManager.Instance.PathMission.GetEntry(placeSignal.PathMissionId);
            if (missionEntry == null)
                throw new InvalidOperationException($"Mission ID not found for ExplorerPlaceSignal: {placeSignal.PathMissionId}");

            Vector3 signalPosition;

            switch ((PathMissionType)missionEntry.PathMissionTypeEnum)
            {
                case PathMissionType.Explorer_Vista:
                    PathExplorerNodeEntry explorerNode = GameTableManager.Instance.PathExplorerNode.Entries
                        .FirstOrDefault(i => i.PathExplorerAreaId == missionEntry.ObjectId);
                    if (explorerNode == null)
                        throw new InvalidOperationException($"ExplorerNode with ID {missionEntry.ObjectId} not found!");

                    WorldLocation2Entry signalLocation = GameTableManager.Instance.WorldLocation2.GetEntry(explorerNode.WorldLocation2Id);
                    if (signalLocation == null)
                        throw new InvalidOperationException($"WorldLocation2 with ID {explorerNode.WorldLocation2Id} not found!");

                    signalPosition = new Vector3(signalLocation.Position0, signalLocation.Position1, signalLocation.Position2);
                    player.PathMissionManager.MissionUpdate(PathMissionType.Explorer_Vista, missionEntry.ObjectId);
                    break;
                default:
                    throw new NotImplementedException($"{(PathMissionType)missionEntry.PathMissionTypeEnum} not supported at this time.");
            }

            ISimpleEntity signal = CreateSimpleEntity(12047, 0u, 23011u);
            signal.CreateFlags = EntityCreateFlag.SpawnAnimation;
            player.Map.EnqueueAdd(signal, signalPosition);
        }

        /// <summary>
        /// Builds the selected settler improvement. Should only be called directly from packet handlers.
        /// </summary>
        public void HandleSettlerBuildImprovement(Player player, ClientPathSettlerImprovementBuildTier buildImprovement)
        {
            if (!settlerImprovementGroups.TryGetValue(buildImprovement.PathSettlerImprovementGroupId, out SettlerImprovementGroup group))
                throw new InvalidOperationException($"SettlerImprovementGroup {buildImprovement.PathSettlerImprovementGroupId} doesn't exist, but it should!");

            group.Build(player, (int)buildImprovement.BuildTier);
        }

        private void SettlerSendImprovementBuildStatus(Player player)
        {
            foreach (PathMission mission in player.PathMissionManager.GetActiveEpisodeMissions())
            {
                if ((Path)mission.Entry.PathTypeEnum != Path.Settler)
                    continue;

                if ((PathMissionType)mission.Entry.PathMissionTypeEnum != PathMissionType.Settler_Hub)
                    continue;

                PathSettlerHubEntry hub = GameTableManager.Instance.PathSettlerHub.GetEntry(mission.Entry.ObjectId);
                if (hub == null)
                    continue;

                var buildStatus = new ServerPathSettlerBuildStatusUpdateList
                {
                    PathSettlerHubId = (ushort)hub.Id
                };

                foreach (PathSettlerImprovementGroupEntry groupEntry in GameTableManager.Instance.PathSettlerImprovementGroup.Entries
                    .Where(g => g.PathSettlerHubId == hub.Id))
                {
                    if (!settlerImprovementGroups.ContainsKey(groupEntry.Id))
                        settlerImprovementGroups.Add(groupEntry.Id, new SettlerImprovementGroup(groupEntry.Id));

                    buildStatus.SettlerImprovementStatuses.Add(settlerImprovementGroups[groupEntry.Id].GetNetworkBuildStatus());
                }

                player.Session.EnqueueMessageEncrypted(buildStatus);
            }
        }

        private static ISimpleEntity CreateSimpleEntity(uint creatureId, uint improvementGroupId, uint? displayInfoId = null)
        {
            IEntityFactory entityFactory = LegacyServiceProvider.Provider.GetRequiredService<IEntityFactory>();
            ICreatureInfoManager creatureInfoManager = LegacyServiceProvider.Provider.GetRequiredService<ICreatureInfoManager>();

            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(creatureId);
            if (displayInfoId.HasValue)
            {
                creatureInfo = new CreatureInfoOverride()
                    .SetCreatureInfoOverride(creatureInfo)
                    .SetDisplayInfoEntryOverride(GameTableManager.Instance.Creature2DisplayInfo.GetEntry(displayInfoId.Value));
            }

            ISimpleEntity entity = entityFactory.CreateEntity<ISimpleEntity>();
            entity.Initialise(creatureInfo);
            entity.ImprovementGroupId = improvementGroupId;
            return entity;
        }
    }
}
