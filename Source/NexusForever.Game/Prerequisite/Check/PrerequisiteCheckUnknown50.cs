using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.Unknown50)]
    public class PrerequisiteCheckUnknown50 : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckUnknown50> log;

        public PrerequisiteCheckUnknown50(
            ILogger<PrerequisiteCheckUnknown50> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            uint spell4Id = value != 0u ? value : objectId;
            if (spell4Id == 0u)
                return false;

            bool hasSpell = player.HasSpell(s => s.Spell4Id == spell4Id, out _);
            return comparison switch
            {
                PrerequisiteComparison.Equal    => hasSpell,
                PrerequisiteComparison.NotEqual => !hasSpell,
                _                               => LogUnhandled(comparison)
            };
        }

        private bool LogUnhandled(PrerequisiteComparison comparison)
        {
            log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.Unknown50}!");
            return false;
        }
    }
}
