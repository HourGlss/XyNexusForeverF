using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.EldanAugmentation227)]
    public class PrerequisiteCheckEldanAugmentation : BasePrerequisiteHandler, IPrerequisiteCheck
    {
        public PrerequisiteCheckEldanAugmentation(
            ILogger<BasePrerequisiteHandler> log)
            : base(log)
        {
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            uint ampId = objectId != 0u ? objectId : value;
            if (ampId > ushort.MaxValue)
                return false;

            bool enabled = player.SpellManager?.IsAmpEnabled((ushort)ampId) ?? false;

            return MatchBoolean(enabled, comparison, PrerequisiteType.EldanAugmentation227);
        }
    }
}
