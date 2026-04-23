using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Adventure.WarOfTheWilds
{
    [ScriptFilterOwnerId(1393)]
    public class WarOfTheWildsMapScript : EventBaseContentMapScript
    {
        public override uint PublicEventId => 158u;
        // TODO: start event based on group faction, 170 is Exile and 171 is Dom
    }
}
