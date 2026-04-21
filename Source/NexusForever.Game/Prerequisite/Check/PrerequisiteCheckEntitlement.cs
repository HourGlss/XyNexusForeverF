using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.Entitlement)]
    public class PrerequisiteCheckEntitlement : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckEntitlement> log;

        public PrerequisiteCheckEntitlement(
            ILogger<PrerequisiteCheckEntitlement> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            return MeetsEntitlement(player, comparison, value, objectId, log, PrerequisiteType.Entitlement);
        }

        internal static bool MeetsEntitlement(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, ILogger log, PrerequisiteType prerequisiteType)
        {
            EntitlementEntry entry = GameTableManager.Instance.Entitlement.GetEntry(objectId);
            if (entry == null)
                throw new ArgumentException($"Invalid entitlement type {objectId}!");

            EntitlementType entitlementType = (EntitlementType)objectId;
            EntitlementFlags flags = (EntitlementFlags)entry.Flags;
            uint currentValue = flags.HasFlag(EntitlementFlags.Character)
                ? (player.EntitlementManager.GetEntitlement(entitlementType)?.Amount ?? 0u)
                : (player.Account.EntitlementManager.GetEntitlement(entitlementType)?.Amount ?? 0u);

            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return currentValue == value;
                case PrerequisiteComparison.NotEqual:
                    return currentValue != value;
                case PrerequisiteComparison.GreaterThanOrEqual:
                    return currentValue >= value;
                case PrerequisiteComparison.GreaterThan:
                    return currentValue > value;
                case PrerequisiteComparison.LessThanOrEqual:
                    return currentValue <= value;
                case PrerequisiteComparison.LessThan:
                    return currentValue < value;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {prerequisiteType}!");
                    return true;
            }
        }
    }
}
