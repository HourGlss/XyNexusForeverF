using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Battleground.DaggerstonePass
{
    [ScriptFilterOwnerId(2166)]
    public class DaggerstonePassMapScript : EventBasePvpContentMapScript
    {
        public override uint PublicEventId    => 438u;
        public override uint PublicSubEventId => 466u;
    }
}