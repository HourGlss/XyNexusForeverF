using NexusForever.Game.Abstract.Combat.CrowdControl;
using NexusForever.Game.Static.Combat.CrowdControl;

namespace NexusForever.Game.Combat.CrowdControl
{
    public class CrowdControlApplyResult : ICrowdControlApplyResult
    {
        public CCStateApplyRulesResult Result { get; set; }
        public TimeSpan? NewDuration { get; set; }
    }
}
