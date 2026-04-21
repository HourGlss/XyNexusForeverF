using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.CostumeUnlocked)]
    public class PrerequisiteCheckCostumeUnlocked : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckCostumeUnlocked> log;

        public PrerequisiteCheckCostumeUnlocked(
            ILogger<PrerequisiteCheckCostumeUnlocked> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return player.Account.CostumeManager.HasItemUnlock(objectId);
                case PrerequisiteComparison.NotEqual:
                    return !player.Account.CostumeManager.HasItemUnlock(objectId);
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.CostumeUnlocked}!");
                    return true;
            }
        }
    }
}
