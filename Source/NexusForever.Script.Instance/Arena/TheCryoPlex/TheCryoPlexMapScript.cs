using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Arena.TheCryoPlex
{
    [ScriptFilterOwnerId(3022)]
    public class TheCryoPlexMapScript : EventBasePvpContentMapScript
    {
        public override uint PublicEventId => 581u;
        public override uint PublicSubEventId => 582u;
    }
}
