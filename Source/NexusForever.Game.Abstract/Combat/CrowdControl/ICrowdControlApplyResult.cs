using NexusForever.Game.Static.Combat.CrowdControl;

namespace NexusForever.Game.Abstract.Combat.CrowdControl
{
    public interface ICrowdControlApplyResult
    {
        CCStateApplyRulesResult Result { get; set; }
        TimeSpan? NewDuration { get; set; }
    }
}
