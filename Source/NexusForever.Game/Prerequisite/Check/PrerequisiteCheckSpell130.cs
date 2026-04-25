using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.Spell130)]
    public class PrerequisiteCheckSpell130 : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckSpell130> log;

        public PrerequisiteCheckSpell130(
            ILogger<PrerequisiteCheckSpell130> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            if (value == 0u)
                return false;

            IUnitEntity unit = parameters?.EvaluateTarget == true && parameters.Target != null
                ? parameters.Target
                : player;

            bool hasSpell = unit.HasSpell(s => s.Spell4Id == value, out _);
            return comparison switch
            {
                PrerequisiteComparison.Equal    => hasSpell,
                PrerequisiteComparison.NotEqual => !hasSpell,
                _                               => LogUnhandled(comparison)
            };
        }

        private bool LogUnhandled(PrerequisiteComparison comparison)
        {
            log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.Spell130}!");
            return false;
        }
    }
}
