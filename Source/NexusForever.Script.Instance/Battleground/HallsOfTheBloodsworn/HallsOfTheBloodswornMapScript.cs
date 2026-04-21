using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Battleground.HallsOfTheBloodsworn
{
    [ScriptFilterOwnerId(3449)]
    public class HallsOfTheBloodswornMapScript : EventBasePvpContentMapScript
    {
        public override uint PublicEventId    => 876u;
        public override uint PublicSubEventId => 877u;
    }
}
