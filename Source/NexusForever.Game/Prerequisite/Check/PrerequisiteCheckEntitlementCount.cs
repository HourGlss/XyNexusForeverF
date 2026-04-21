using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.EntitlementCount)]
    public class PrerequisiteCheckEntitlementCount : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckEntitlementCount> log;

        public PrerequisiteCheckEntitlementCount(
            ILogger<PrerequisiteCheckEntitlementCount> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            return PrerequisiteCheckEntitlement.MeetsEntitlement(player, comparison, value, objectId, log, PrerequisiteType.EntitlementCount);
        }
    }
}
