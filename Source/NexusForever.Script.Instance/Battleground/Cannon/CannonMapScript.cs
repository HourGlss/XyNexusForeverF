using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Battleground.Cannon
{
    [ScriptFilterOwnerId(921)]
    public class CannonMapScript : EventBasePvpContentMapScript
    {
        public override uint PublicEventId    => 0u;//unknown
        public override uint PublicSubEventId => 469u;
    }
}