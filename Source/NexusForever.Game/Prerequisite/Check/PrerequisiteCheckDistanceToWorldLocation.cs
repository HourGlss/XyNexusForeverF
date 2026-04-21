using System.Numerics;
using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.DistanceToWorldLocation)]
    public class PrerequisiteCheckDistanceToWorldLocation : BasePrerequisiteHandler, IPrerequisiteCheck
    {
        private readonly IGameTableManager gameTableManager;

        public PrerequisiteCheckDistanceToWorldLocation(
            ILogger<BasePrerequisiteHandler> log,
            IGameTableManager gameTableManager)
            : base(log)
        {
            this.gameTableManager = gameTableManager;
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            WorldLocation2Entry worldLocation = gameTableManager.WorldLocation2.GetEntry(objectId);
            if (worldLocation == null)
            {
                log.LogWarning($"WorldLocation2Entry {objectId} not found for {PrerequisiteType.DistanceToWorldLocation}.");
                return false;
            }

            var worldPosition = new Vector3(worldLocation.Position0, worldLocation.Position1, worldLocation.Position2);
            float distance = player.GetDistanceTo(worldPosition);
            float requiredDistance = unchecked((int)value);

            return MatchComparable(distance, requiredDistance, comparison, PrerequisiteType.DistanceToWorldLocation);
        }
    }
}
